namespace ThreadsWpf;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Char;
using static System.Windows.Input.Keyboard;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class WindowAccount : Window
{
    Account? _myAccount;
    bool _myClosing = false;
    bool _stopMessageBox = false;

    public static readonly DependencyProperty BalanceProp = DependencyProperty.Register(nameof(Balance), typeof(int), typeof(WindowAccount));
    int Balance { get => (int)GetValue(BalanceProp); set => SetValue(BalanceProp, value); }

    public static readonly DependencyProperty ActiveProp = DependencyProperty.Register(nameof(Active), typeof(bool), typeof(WindowAccount));
    bool Active { get => (bool)GetValue(ActiveProp); set => SetValue(ActiveProp, value); }

    public WindowAccount()
    {
        Active = true;
        InitializeComponent();
    }

    enum Examples : uint { Buggy, Fixed, Conditional }
    // change the initialization for appropriate example Buggy => Fixed => Conditional
    static readonly Examples s_example = Examples.Conditional;

    void windowAccountObserver(object? sender, AccountEventArgs args)
    {
        switch (s_example)
        {
            // Before changing to Dispatcher - crashes with InvalidOperation
            case Examples.Buggy: updateBalance(args.Balance); break;
            case Examples.Fixed: updateBalanceThreadSafe(args.Balance); break;
            case Examples.Conditional: updateBalanceCondThreadSafe(args.Balance); break;
        };
    }

    void updateBalance(int balance)
    {
        if (balance != Account.CLOSED) // special value meaning that the Account is going to close
            Balance = balance;
        else
        {   // Prepare for window closing...
            _myAccount!.BalanceChanged -= windowAccountObserver;
            _myAccount = null;
            _myClosing = true;
            // Let application to close after the window is closed
            Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            // Close our window
            Close();
            // If you will do it from another file with User Control or something like
            // that, do it the following way
            // Window.GetWindow(this).Close();
        }
    }

    void updateBalanceThreadSafe(int balance)
    // Microsoft recommendation is not using CheckAccess() but simply doing as follows:
    // => Dispatcher.BeginInvoke((Action<int>)(x => updateBalance(x)), balance);
    // however it forces going through event loop even when it's not required (if we are already in the UI thread)
    {
        if (CheckAccess())
            updateBalance(balance);
        else
            Dispatcher.BeginInvoke((Action<int>)(x => updateBalance(x)), balance);
    }

    // Example for avoiding closint the window...
    void window_Closing(object sender, CancelEventArgs e)
    {
        if (_myClosing)
        {
            if (_myAccount is not null)
            {
                _myAccount.Close(false);
                _myAccount.ThreadFinished(true);
            }
        }
        else // Won't allow to cancel the window!!! It is not me!!!
        {
            e.Cancel = true;
            MessageBox.Show(@"DON""T CLOSE ME!!!", "STOP IT!!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    void window_Loaded(object sender, RoutedEventArgs e)
    {
        _myAccount = new Account(1000, 2);
        _myAccount.BalanceChanged += windowAccountObserver;
    }

    void btnStop_Click(object sender, RoutedEventArgs e)
    {
        if (_myAccount is not null)
            new Thread(() => _myAccount.Close(true)).Start();
        _stopMessageBox = true;
        Active = false;
    }

    void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox text = (TextBox)sender;
        if (text is null || e is null) return;

        switch (e.Key)
        {
            case Key.Enter:
                if (text.Text.Length > 0)
                {
                    int amount = int.Parse(text.Text);
                    text.Text = "";
                    if (sender == txtDeposit)
                        _myAccount!.Deposit(amount);
                    else if (sender == txtWithdraw)
                        if (!_myAccount!.Withdraw(amount))
                            MessageBox.Show("You do not have enough money!", "Account",
                                            MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                e.Handled = true;
                return;

            // It`s a system key (add other key here if you want to allow)
            case var key when key.In(Key.Escape, Key.Back, Key.Delete, Key.CapsLock,
                                     Key.LeftShift, Key.RightShift, Key.LeftCtrl, Key.RightCtrl,
                                     Key.LeftAlt, Key.RightAlt, Key.LWin, Key.RWin,
                                     Key.System, Key.Left, Key.Up, Key.Down, Key.Right):
                return;
        }

        char c = (char)KeyInterop.VirtualKeyFromKey(e.Key);
        if (IsControl(c) ||
           (IsDigit(c) && !(IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift) ||
                            IsKeyDown(Key.LeftCtrl) || IsKeyDown(Key.RightCtrl) ||
                            IsKeyDown(Key.LeftAlt) || IsKeyDown(Key.RightAlt)))) return;

        e.Handled = true;
        MessageBox.Show("Only numbers are allowed", "Account", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    bool updateBalanceCond(int balance)
    {
        updateBalance(balance);
        return !_stopMessageBox && balance < 500;
    }

    void updateBalanceCondThreadSafe(int balance) =>
        // Microsoft recommendation is not using CheckAccess() but simply doing as follows:
        // warnLowBalance((bool)Dispatcher.Invoke((Predicate<int>)(x => updateBalanceCond(x)), balance));
        // however it forces going through event loop even when it's not required (if we are already in the UI thread)
        warnLowBalance(CheckAccess() ? updateBalanceCond(balance)
                                     : (bool)Dispatcher.Invoke((Predicate<int>)(x => updateBalanceCond(x)), balance)
                      );

    bool _warned = false;
    void warnLowBalance(bool check)
    {
        if (check)
        {
            if (!_warned)
            {
                _warned = true;
                new Thread(() => MessageBox.Show("You are going low on balance!",
                                                  "Account warning",
                                                  MessageBoxButton.OK,
                                                  MessageBoxImage.Exclamation)
                          ).Start(); 
            }
        }
        else
            _warned = false;
    }
}

static class Tools
{
    internal static bool In(this Key val, params Key[] vals) => vals.Contains(val);
}

namespace ThreadsWpfTask;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for WindowAccount.xaml
/// </summary>
public partial class WindowAccount : Window
{
    Account? _myAccount;
    bool _myClosing = false;

    public static readonly DependencyProperty BalanceProp = DependencyProperty.Register(nameof(Balance), typeof(int), typeof(WindowAccount));
    int Balance { get => (int)GetValue(BalanceProp); set => SetValue(BalanceProp, value); }

    public static readonly DependencyProperty ActiveProp = DependencyProperty.Register(nameof(Active), typeof(bool), typeof(WindowAccount));
    bool Active { get => (bool)GetValue(ActiveProp); set => SetValue(ActiveProp, value); }

    public WindowAccount()
    {
        Active = true;
        InitializeComponent();
    }

    void windowAccountObserver(object? sender, AccountEventArgs args) => updateBalance(args.Balance);

    void updateBalance(int balance) => Balance = balance;

    // Example for avoiding closing the window...
    void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (!_myClosing) // Won't allow to cancel the window!!! It is not me!!!
        {
            e.Cancel = true;
            MessageBox.Show(@"DON""T CLOSE ME!!!", "STOP IT!!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    async void window_Loaded(object sender, RoutedEventArgs e)
    {
        Account.BalanceChanged += windowAccountObserver;
        _myAccount = new Account(1000, 2);
        await _myAccount.RunInterestTask(); // Run task of interest without blocking (see inside)
        // We continue here only after the interest task finished
        _myClosing = true;
        Account.BalanceChanged -= windowAccountObserver;
        _myAccount = null;
        // Prepare for window closing...
        // Let application to close after the window is closed
        Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
        // Close our window
        Close();
        // If you will do it from another file with User Control or something like
        // that, do it the following way
        // Window.GetWindow(this).Close();
    }

    private void btnStop_Click(object sender, RoutedEventArgs e)
    {
        _myAccount?.Close();
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
        if (Char.IsControl(c) ||
           (Char.IsDigit(c) && !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
                                 Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                                 Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))) return;

        e.Handled = true;
        MessageBox.Show("Only numbers are allowed", "Account", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

static class Tools
{
    internal static bool In(this Key val, params Key[] vals) => vals.Contains(val);
}

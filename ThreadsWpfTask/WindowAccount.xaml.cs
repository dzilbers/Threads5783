﻿namespace ThreadsWpfTask;
using System.Windows.Controls;
using System.Windows.Input;

/// <summary>
/// Interaction logic for WindowAccount.xaml
/// </summary>
public partial class WindowAccount : Window
{
    public WindowAccount() => InitializeComponent();

    Account? _myAccount;
    bool _myClosing = false;

    void windowAccountObserver(object? sender, AccountEventArgs args) => updateBalance(args.Balance);

    void updateBalance(int balance) => txtBalance.Content = String.Format("{0}", balance);

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
        if (_myAccount != null)
        {
            _myAccount.Close();
            Button button = (Button)sender;
            button.Content = "Closing";
            button.IsEnabled = false;
        }
        //Form2.Form2Create();
    }
    private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox text = (TextBox)sender;
        if (text == null) return;
        if (e == null) return;

        if (e.Key == Key.Enter || e.Key == Key.Return)
        {
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
        }

        // It`s a system key (add other key here if you want to allow)
        if (e.Key == Key.Escape || e.Key == Key.Tab || e.Key == Key.Back || e.Key == Key.Delete ||
            e.Key == Key.CapsLock || e.Key == Key.LeftShift || e.Key == Key.RightShift ||
            e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftAlt ||
            e.Key == Key.RightAlt || e.Key == Key.LWin || e.Key == Key.RWin || e.Key == Key.System ||
            e.Key == Key.Left || e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Right)
            return;

        char c = (char)KeyInterop.VirtualKeyFromKey(e.Key);
        if (Char.IsControl(c)) return;
        if (Char.IsDigit(c))
            if (!(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift) ||
                  Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) ||
                  Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                return;
        e.Handled = true;
        MessageBox.Show("Only numbers are allowed", "Account", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

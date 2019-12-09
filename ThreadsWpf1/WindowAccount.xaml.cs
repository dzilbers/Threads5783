using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ThreadsWpf1
{
    /// <summary>
    /// Interaction logic for WindowAccount.xaml
    /// </summary>
    public partial class WindowAccount : Window
    {
        public WindowAccount()
        {
            InitializeComponent();
        }

        private Account myAccount;
        private bool myClosing = false;
        private bool stopMessageBox = false;

        private void windowAccountObserver(object sender, AccountEventArgs args)
        {
            // Before changing to Dispatcher - crashes with InvalidOperation
            // Comment it out
            //updateBalance(args.Balance);
            // Uncomment one of the next two lines it upon commenting out the above line
            //UpdateBalance(args.Balance);
            UpdateBalanceCond(args.Balance);
            //UpdateBalance2(args.Balance);
            //UpdateBalance3(args.Balance);

        }

        private void updateBalance(int balance)
        {
            if (balance != Account.CLOSED) // special value meaning that the Account is going to close
                txtBalance.Content = String.Format("{0}", balance);
            else
            {   // Prepare for window closing...
                myAccount.BalanceChanged -= windowAccountObserver;
                myAccount = null;
                myClosing = true;
                // Let application to close after the window is closed
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                // Close our window
                Close();
                // If you will do it from another file with User Control or something like
                // that, do it the following way
                // Window.GetWindow(this).Close();
            }
        }

        // Uncomment it when going to WPF Dispatcher usage...
        public void UpdateBalance(int balance)
        {
            if (CheckAccess())
                updateBalance(balance);
            else
                Dispatcher.BeginInvoke((Action<int>)
                                   (x => updateBalance(x)), balance);
        }

        public void UpdateBalance2(int balance)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => updateBalance((int)args.Argument);
            //worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            //worker.WorkerReportsProgress = true;
            //worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync(balance);
        }

        public async void UpdateBalance3(int balance)
        {
            await Task.Run(() => updateBalance(balance));
        }

        // Example for avoiding closint the window...
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (myClosing)
            {
                if (myAccount != null)
                {
                    myAccount.Close(false);
                    myAccount.threadFinished(true);
                }
            }
            else // Won't allow to cancel the window!!! It is not me!!!
            {
                e.Cancel = true;
                MessageBox.Show(@"DON""T CLOSE ME!!!", "STOP IT!!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            myAccount = new Account(1000, 2);
            myAccount.BalanceChanged += windowAccountObserver;
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (myAccount != null)
                new Thread(() => myAccount.Close(true)).Start();
            stopMessageBox = true;
            Button button = sender as Button;
            button.Content = "Closing";
            button.IsEnabled = false;
            lblBalance.Content = "Closing in:";
            //Form2.Form2Create();
        }
        private void textBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox text = sender as TextBox;
            if (text == null) return;
            if (e == null) return;

            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                if (text.Text.Length > 0)
                {
                    int amount = int.Parse(text.Text);
                    text.Text = "";
                    if (sender == txtDeposit)
                        myAccount.Deposit(amount);
                    else if (sender == txtWithdraw)
                        if (!myAccount.Withdraw(amount))
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

        private bool updateBalanceCond(int balance)
        {
            updateBalance(balance);
            return stopMessageBox ? false : balance < 500;
        }

        private void UpdateBalanceCond(int balance)
        {
            warnLowBalance(CheckAccess() ? updateBalanceCond(balance) :
                           (bool)Dispatcher.Invoke((Predicate<int>)(x => updateBalanceCond(x)), balance)
                          );
        }

        public void UpdateBalanceCond2(int balance)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => { args.Result = updateBalanceCond((int)args.Argument); };
            worker.RunWorkerCompleted += (sender, args) => warnLowBalance((bool)args.Result);
            //worker.WorkerReportsProgress = true;
            //worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync(balance);
        }

        public async void UpdateBalanceCond3(int balance)
        {
            warnLowBalance(await Task<bool>.Run(() => updateBalanceCond(balance)));
        }

        private bool warned = false;
        private void warnLowBalance(bool check)
        {
            if (check)
            {
                if (!warned)
                {
                    warned = true;
                    new Thread(() => MessageBox.Show("You are going low on balance!",
                                                      "Account warning",
                                                      MessageBoxButton.OK,
                                                      MessageBoxImage.Exclamation)
                              ).Start();
                }
            }
            else
                warned = false;
        }

        private static int counter = 0;
        private void btnOther_Click(object sender, RoutedEventArgs e)
        {
            new OtherWindow(++counter).Show();
            //OtherWindow.Create("Thread " + ++counter, counter);
        }
    }
}

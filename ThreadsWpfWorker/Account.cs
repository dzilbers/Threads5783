using System;
using System.ComponentModel;
using System.Threading;

namespace ThreadsWpfWorker
{
    class Account
    {
        private static readonly Account? s_account = null;

        public static event EventHandler<AccountEventArgs>? AccountClosed;
        private static void accountClosedHandler(object result) => AccountClosed?.Invoke(s_account, new AccountEventArgs((int)result));//if (AccountClosed != null)//    AccountClosed(account, new EventArgs());

        public static event EventHandler<AccountEventArgs>? BalanceChanged;
        private static void balanceChangedHandler(int balance) => BalanceChanged?.Invoke(s_account, new AccountEventArgs(balance));

        private int _balance;
        private int Balance
        {
            get { return _balance; }
            set
            {
                if (_balance != value)
                {
                    _balance = value;
                    balanceChangedHandler(value);
                }
            }
        }

        private readonly int _interestRate; // integer % number
        //private volatile bool _shouldStop;
        private Thread? _myThread = null;
        private readonly BackgroundWorker _worker = new();
        public Account(int initBalance, int interestRate)
        {
            Balance = initBalance;
            this._interestRate = interestRate;
            //BackgroundWorker worker = new BackgroundWorker();
            _worker.RunWorkerCompleted += (sender, args) => accountClosedHandler(args.Result!);
            _worker.WorkerReportsProgress = true;
            _worker.ProgressChanged += (sender, args)
                => Balance = args.ProgressPercentage == 1 ? (int)args.UserState! : 100 - args.ProgressPercentage;
            _worker.WorkerSupportsCancellation = true;
            _worker.DoWork += (sender, args) =>
            {
                _myThread = Thread.CurrentThread;
                //_shouldStop = false;
                try { Thread.Sleep(3000); } catch (ThreadInterruptedException) { } // 3 secs
                while (!_worker.CancellationPending) //(!_shouldStop)
                {
                    applyInterest(); // worker.ReportProgress(1);
                    try { Thread.Sleep(3000); } catch (ThreadInterruptedException) { } // 3 secs
                }
                _worker.ReportProgress(95);
                for (int p = 96; p <= 100; ++p) // 5 secs delay
                {
                    Thread.Sleep(1000);
                    _worker.ReportProgress(p);
                }
                args.Result = -999;
            };
            _worker.RunWorkerAsync();
        }

        public void Deposit(int amount) => Balance += amount;

        public bool Withdraw(int amount)
        {
            if (amount > Balance) return false;
            Balance -= amount;
            return true;
        }

        private void applyInterest() => _worker.ReportProgress(1, (Balance * (100 + _interestRate)) / 100);

        public void Close()
        {
            //_shouldStop = true;
            _worker.CancelAsync();
            _myThread!.Interrupt();
        }
    }
}

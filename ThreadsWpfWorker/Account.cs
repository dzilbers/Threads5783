using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadsWpfWorker
{
    class Account
    {
        private static Account account = null;

        public static event EventHandler<AccountEventArgs> AccountClosed;
        private static void accountClosedHandler(object result)
        {
            AccountClosed?.Invoke(account, new AccountEventArgs((int)result));
            //if (AccountClosed != null)
            //    AccountClosed(account, new EventArgs());
        }

        public static event EventHandler<AccountEventArgs> BalanceChanged;
        private static void balanceChangedHandler(int balance)
        {
            BalanceChanged?.Invoke(account, new AccountEventArgs(balance));
        }

        private int balance;
        private int Balance
        {
            get { return balance; }
            set
            {
                if (balance != value)
                {
                    balance = value;
                    balanceChangedHandler(value);
                }
            }
        }

        private readonly int interestRate; // integer % number
        //private volatile bool _shouldStop;
        private Thread myThread = null;
        private BackgroundWorker worker = new BackgroundWorker();
        public Account(int initBalance, int interestRate)
        {
            this.Balance = initBalance;
            this.interestRate = interestRate;
            //BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, args) => accountClosedHandler(args.Result);
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (sender, args)
                => Balance = args.ProgressPercentage == 1 ? (int)args.UserState : 100 - args.ProgressPercentage;
            //{
            //  if (args.ProgressPercentage == 1)
            //     applyInterest();
            //  else
            //    Balance = 100 - args.ProgressPercentage;
            //};
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += (sender, args) =>
            {
                myThread = Thread.CurrentThread;
                //_shouldStop = false;
                try { Thread.Sleep(3000); } catch (ThreadInterruptedException) { } // 3 secs
                while (!worker.CancellationPending) //(!_shouldStop)
                {
                    applyInterest(); // worker.ReportProgress(1);
                    try { Thread.Sleep(3000); } catch (ThreadInterruptedException) { } // 3 secs
                }
                worker.ReportProgress(95);
                for (int p = 96; p <= 100; ++p) // 5 secs delay
                {
                    Thread.Sleep(1000);
                    worker.ReportProgress(p);
                }
                args.Result = -999;
            };
            worker.RunWorkerAsync();
        }

        public void Deposit(int amount)
        {
            Balance += amount;
        }

        public bool Withdraw(int amount)
        {
            if (amount > Balance) return false;
            Balance -= amount;
            return true;
        }

        private void applyInterest()
        {
            worker.ReportProgress(1, (Balance * (100 + interestRate)) / 100);
        }

        public void Close()
        {
            //_shouldStop = true;
            worker.CancelAsync();
            myThread.Interrupt();
        }
    }
}

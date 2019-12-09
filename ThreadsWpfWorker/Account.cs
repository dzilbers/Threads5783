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

        public static event EventHandler<EventArgs> AccountClosed;
        private static void accountClosedHandler()
        {
            AccountClosed?.Invoke(account, new EventArgs());
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
        private volatile bool _shouldStop;
        private Thread myThread = null;
        public Account(int initBalance, int interestRate)
        {
            this.Balance = initBalance;
            this.interestRate = interestRate;
            BackgroundWorker worker = new BackgroundWorker();
            worker.RunWorkerCompleted += (sender, args) => accountClosedHandler();
            worker.WorkerReportsProgress = true;
            worker.ProgressChanged += (sender, args) =>
            {
                if (args.ProgressPercentage == 1)
                    applyInterest();
                else
                    Balance = 100 - args.ProgressPercentage;
            };
            worker.DoWork += (sender, args) =>
            {
                myThread = Thread.CurrentThread;
                _shouldStop = false;
                try { Thread.Sleep(3000); } catch (Exception) { } // 3 secs
                while (!_shouldStop)
                {
                    worker.ReportProgress(1);
                    try { Thread.Sleep(3000); } catch (Exception) { } // 3 secs
                }
                worker.ReportProgress(95);
                for (int p = 96; p <= 100; ++p) // 5 secs delay
                {
                    Thread.Sleep(1000);
                    worker.ReportProgress(p);
                }
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
            Balance = (Balance * (100 + interestRate)) / 100;
        }

        public void Close()
        {
            _shouldStop = true;
            myThread.Interrupt();
        }
    }
}

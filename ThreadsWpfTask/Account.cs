using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreadsWpfTask
{
    class Account
    {
        private static Account account = null;

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
            account = this;
            this.Balance = initBalance;
            this.interestRate = interestRate;
        }

        public async Task RunInterestTask()
        {
            // the progress object must be produced in the context of UI thread
            IProgress<int> progress = new Progress<int>(p =>
            {
                if (p == 1)
                    applyInterest();
                else
                    Balance = 100 - p;
            });
            await Task.Run(() => interestLoop(progress));
        }

        public void interestLoop(IProgress<int> progress)
        {
            myThread = Thread.CurrentThread;
            _shouldStop = false;
            try { Thread.Sleep(3000); } catch (Exception) { } // 3 secs
            while (!_shouldStop)
            {
                progress.Report(1);
                try { Thread.Sleep(3000); } catch (Exception) { } // 3 secs
            }
            progress.Report(95);
            for (int p = 96; p <= 100; ++p) // 5 secs delay
            {
                Thread.Sleep(1000);
                progress.Report(p);
            }
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

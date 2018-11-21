using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ThreadsWpf1
{
    class Account
    {
        public enum AccountState { RUNNING, STOPCLOSED, STOP }
        public const int CLOSED = -999999;

        public event EventHandler<AccountEventArgs> BalanceChanged;
        void BalanceChangedHandler(int balance)
        {
            if (BalanceChanged != null)
            {
                new Thread((obj) => BalanceChanged(this, (AccountEventArgs)obj)
                          ).Start(new AccountEventArgs(balance));
            }
        }

        private int balance;
        private int Balance
        {
            get { return balance; }
            set
            {
                if (balance == CLOSED) return;
                if (balance != value)
                {
                    balance = value;
                    BalanceChangedHandler(value);
                }
            }
        }

        private readonly int interestRate; // integer % number
        private Thread myThread;
        private volatile AccountState _shouldStop;
        public Account(int initBalance, int interestRate)
        {
            this.Balance = initBalance;
            this.interestRate = interestRate;
            new Thread(() =>
            {
                myThread = Thread.CurrentThread;
                _shouldStop = AccountState.RUNNING;
                while (_shouldStop == AccountState.RUNNING)
                {
                    applyInterest();
                    Thread.Sleep(3000); // 3 secs
                }
                for (Balance = 5; Balance > 0; Balance--)
                {
                    Thread.Sleep(1000); // 5 secs delay
                }
                if (_shouldStop == AccountState.STOPCLOSED)
                    Balance = CLOSED;
            }).Start();
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

        public void Close(bool upd)
        {
            _shouldStop = upd ? AccountState.STOPCLOSED : AccountState.STOP;
        }

        public bool threadFinished(bool sync)
        {
            if (myThread == null)
                return true;
            if (sync)
            {
                myThread.Join();
                return true;
            }
            bool t = !myThread.IsAlive;
            return t;
        }
    }
}

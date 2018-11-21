using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreadsWpfWorker
{
    class AccountEventArgs : EventArgs
    {
        private int balance;
        public int Balance { get { return balance; } private set { balance = value; } }
        public AccountEventArgs(int balance)
        {
            Balance = balance;
        }
    }
}

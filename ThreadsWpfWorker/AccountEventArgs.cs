using System;
using System.Collections.Generic;
using System.Linq;
namespace ThreadsWpfWorker;

class AccountEventArgs : EventArgs
{
    public int Balance { get; private set; }
    public AccountEventArgs(int balance) => Balance = balance;
}

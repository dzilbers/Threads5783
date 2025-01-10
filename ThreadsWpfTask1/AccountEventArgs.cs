namespace ThreadsWpfTask2;

class AccountEventArgs : EventArgs
{
    public int Balance { get; private set; }
    public AccountEventArgs(int balance) => Balance = balance;
}

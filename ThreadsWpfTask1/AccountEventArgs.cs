namespace ThreadsWpfTask1;

class AccountEventArgs(int balance) : EventArgs
{
    public int Balance { get; private set; } = balance;
}

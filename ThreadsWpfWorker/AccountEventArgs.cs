namespace ThreadsWpfWorker;

class AccountEventArgs(int balance) : EventArgs
{
    public int Balance { get; private set; } = balance;
}

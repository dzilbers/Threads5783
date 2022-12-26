using System.Runtime.CompilerServices;

namespace ThreadsWpfTask;

class Account
{
    readonly int _initBalance;
    int _balance;
    int Balance
    {
        get => _balance;
        set { if (_balance != value) balanceChangedHandler(_balance = value); }
    }

    readonly int _interestRate; // integer % number

    public event EventHandler<AccountEventArgs>? BalanceChanged;
    void balanceChangedHandler(int balance) => BalanceChanged?.Invoke(this, new AccountEventArgs(balance));

    Thread? _myThread = null;
    volatile bool _shouldStop;

    public Account(int initBalance, int interestRate)
    {
        _initBalance = initBalance;
        _interestRate = interestRate;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Deposit(int amount) => Balance += amount;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Withdraw(int amount)
    {
        if (amount > Balance) return false;
        Balance -= amount;
        return true;
    }

    public async Task RunInterestTask()
    {
        // the progress object must be produced in the context of UI thread
        IProgress<int> progress = new Progress<int>(p =>
        {
            if (p == 100)
                applyInterest();
            else
                countDown(p);
        });
        await Task.Run(() => interestLoop(progress));
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    void applyInterest() => Balance = (Balance * (100 + _interestRate)) / 100;

    [MethodImpl(MethodImplOptions.Synchronized)]
    void countDown(int amount) => Balance = amount;

    void interestLoop(IProgress<int> progress)
    {
        _myThread = Thread.CurrentThread;
        Balance = _initBalance;
        _shouldStop = false;
        sleep(3);
        while (!_shouldStop)
        {
            progress.Report(100);
            sleep(3);
        }
        for (int p = 5; p >= 0; --p) // 5 secs delay
        {
            progress.Report(p);
            sleep(1);
        }
    }

    public void Close()
    {
        _shouldStop = true;
        _myThread?.Interrupt();
    }

    static void sleep(double seconds)
    {
        try { Thread.Sleep((int)(seconds * 1000)); } catch (ThreadInterruptedException) { }
    }
}

using System.Runtime.CompilerServices;

namespace ThreadsWpfTask2;

class Account(int initBalance, int interestRate)
{
    readonly int _initBalance = initBalance;
    readonly int _interestRate = interestRate; // integer % number

    int _balance;
    int Balance
    {
        [MethodImpl(MethodImplOptions.Synchronized)]
        get => _balance;
        [MethodImpl(MethodImplOptions.Synchronized)]
        set { if (_balance != value) balanceChangedHandler(_balance = value); }
    }

    public event EventHandler<AccountEventArgs>? BalanceChanged;
    void balanceChangedHandler(int balance) => BalanceChanged?.Invoke(this, new AccountEventArgs(balance));

    Thread? _myThread = null;
    volatile bool _shouldStop;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Deposit(int amount) => Balance += amount;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Withdraw(int amount)
    {
        if (amount > Balance) return false;
        Balance -= amount;
        return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    void applyInterest() => Balance = (Balance * (100 + _interestRate)) / 100;

    [MethodImpl(MethodImplOptions.Synchronized)]
    void countDown(int amount) => Balance = amount;

    public async Task StartAsync() => await Task.Run(() => interestLoop());

    void interestLoop()
    {
        _myThread = Thread.CurrentThread;
        Balance = _initBalance;
        _shouldStop = false;
        sleep(3);
        while (!_shouldStop)
        {
            applyInterest();
            sleep(3);
        }
        for (int p = 5; p >= 0; --p) // 5 secs delay
        {
            countDown(p);
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

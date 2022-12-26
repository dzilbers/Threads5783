namespace ThreadsWpfTask;

class Account
{
    private static Account? s_account = null;

    public static event EventHandler<AccountEventArgs>? BalanceChanged;
    private static void balanceChangedHandler(int balance) => BalanceChanged?.Invoke(s_account, new AccountEventArgs(balance));

    private int _balance;
    private int Balance
    {
        get => _balance;
        set
        {
            if (_balance != value)
            {
                _balance = value;
                balanceChangedHandler(value);
            }
        }
    }

    private readonly int _interestRate; // integer % number
    private volatile bool _shouldStop;
    private Thread? _myThread = null;
    public Account(int initBalance, int interestRate)
    {
        s_account = this;
        Balance = initBalance;
        _interestRate = interestRate;
    }

    public void Deposit(int amount) => Balance += amount;

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
            if (p == 1)
                applyInterest();
            else
                Balance = 100 - p;
        });
        await Task.Run(() => interestLoop(progress));
    }

    void applyInterest() => Balance = (Balance * (100 + _interestRate)) / 100;
    void interestLoop(IProgress<int> progress)
    {
        _myThread = Thread.CurrentThread;
        _shouldStop = false;
        sleep(3.0); // 3 secs
        while (!_shouldStop)
        {
            progress.Report(1);
            sleep(3); // 3 secs
        }
        progress.Report(95);
        for (int p = 96; p <= 100; ++p) // 5 secs delay
        {
            sleep(1);
            progress.Report(p);
        }
    }

    public void Close()
    {
        _shouldStop = true;
        _myThread?.Interrupt();
    }

    static void sleep(double seconds)
    {
        try { Thread.Sleep((int)(seconds * 1000)); } catch (Exception) { }
    }
}

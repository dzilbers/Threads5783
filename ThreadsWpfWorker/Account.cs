namespace ThreadsWpfWorker;
using System.ComponentModel;
using System.Runtime.CompilerServices;

class Account
{

    public event EventHandler<AccountEventArgs>? AccountClosed;
    void accountClosedHandler(int result) => AccountClosed?.Invoke(this, new AccountEventArgs(result));

    public event EventHandler<AccountEventArgs>? BalanceChanged;
    void balanceChangedHandler(int balance) => BalanceChanged?.Invoke(this, new AccountEventArgs(balance));

    int _balance;
    int Balance
    {
        get => _balance;
        set { if (_balance != value) balanceChangedHandler(_balance = value); }
    }

    readonly int _interestRate; // integer % number
    
    //private volatile bool _shouldStop;
    private Thread? _myThread = null;
    private readonly BackgroundWorker _worker = new();
    public Account(int initBalance, int interestRate)
    {
        _interestRate = interestRate;
        _worker.RunWorkerCompleted += (sender, args) => accountClosedHandler((int)args.Result!);
        _worker.WorkerReportsProgress = true;
        _worker.ProgressChanged += (sender, args) => { if (args.ProgressPercentage == 1) applyInterest(); else countDown((int)args.UserState!); };
        _worker.WorkerSupportsCancellation = true;
        _worker.DoWork += (sender, args) =>
        {
            _myThread = Thread.CurrentThread;
            Balance = (int)(args.Argument ?? 0);
            //_shouldStop = false;
            sleep(3);
            while (!_worker.CancellationPending) //(!_shouldStop)
            {
                _worker.ReportProgress(1);
                sleep(3);
            }
            for (int p = 5; p >= 0; --p) // 5 secs delay
            {
                _worker.ReportProgress(0, p);
                sleep(1);
            }
            args.Result = -999;
        };
        _worker.RunWorkerAsync(initBalance);
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    void applyInterest() => Balance = (Balance * (100 + _interestRate)) / 100;

    [MethodImpl(MethodImplOptions.Synchronized)]
    void countDown(int amount) => Balance = amount;

    public void Close()
    {
        //_shouldStop = true;
        _worker.CancelAsync();
        _myThread?.Interrupt();
    }

    static void sleep(double seconds)
    {
        try { Thread.Sleep((int)(seconds * 1000)); } catch (ThreadInterruptedException) { }
    }
}

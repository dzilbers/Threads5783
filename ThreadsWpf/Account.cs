namespace ThreadsWpf;
using System.Runtime.CompilerServices;

class Account
{
    int _balance;
    int Balance
    {
        get => _balance;
        set { if (_balance != CLOSED && _balance != value) balanceChangedHandler(_balance = value); }
    }

    readonly int _interestRate; // integer % number

    Thread? _myThread;
    public enum AccountState { RUNNING, STOPCLOSED, STOP }
    private volatile AccountState _shouldStop;
    public const int CLOSED = -999999;

    public event EventHandler<AccountEventArgs>? BalanceChanged;
    void balanceChangedHandler(int balance) => new Thread(
             (arg) => BalanceChanged?.Invoke(this, (AccountEventArgs)arg!)
         ).Start(new AccountEventArgs(balance));

    public Account(int initBalance, int interestRate)
    {
        this._interestRate = interestRate;
        new Thread(() =>
        {
            Balance = initBalance;
            _myThread = Thread.CurrentThread;
            _shouldStop = AccountState.RUNNING;
            sleep(3);
            while (_shouldStop == AccountState.RUNNING)
            {
                applyInterest();
                sleep(3);
            }
            for (int count = 5; count >= 0; --count)
            {
                countDown(count);
                sleep(1); // 5 secs delay
            }
            if (_shouldStop == AccountState.STOPCLOSED) setClosed();
        }).Start();
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

    [MethodImpl(MethodImplOptions.Synchronized)]
    void setClosed() => Balance = CLOSED;

    public void Close(bool upd)
    {
        _shouldStop = upd ? AccountState.STOPCLOSED : AccountState.STOP;
        _myThread?.Interrupt();
    }

    public bool ThreadFinished(bool sync)
    {
        if (_myThread is null)
            return true;

        if (sync)
        {
            _myThread.Join();
            return true;
        }
        bool t = !_myThread.IsAlive;
        return t;
    }

    static void sleep(double seconds)
    {
        try { Thread.Sleep((int)(seconds * 1000)); } catch (ThreadInterruptedException) { }
    }
}

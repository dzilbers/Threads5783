namespace ThreadsWpf;

class Account
{
    public enum AccountState { RUNNING, STOPCLOSED, STOP }
    public const int CLOSED = -999999;

    public event EventHandler<AccountEventArgs>? BalanceChanged;
    void balanceChangedHandler(int balance) =>
        new Thread((obj) => BalanceChanged?.Invoke(this, (AccountEventArgs)obj!)
                  ).Start(new AccountEventArgs(balance));
    //BalanceChanged(this, new AccountEventArgs(balance));

    private int _balance;
    private int Balance
    {
        get { return _balance; }
        set
        {
            if (_balance == CLOSED) return;
            if (_balance != value)
            {
                _balance = value;
                balanceChangedHandler(value);
            }
        }
    }

    private readonly int _interestRate; // integer % number
    private Thread? _myThread;
    private volatile AccountState _shouldStop;
    public Account(int initBalance, int interestRate)
    {
        Balance = initBalance;
        this._interestRate = interestRate;
        new Thread(() =>
        {
            _myThread = Thread.CurrentThread;
            _shouldStop = AccountState.RUNNING;
            while (_shouldStop == AccountState.RUNNING)
            {
                applyInterest();
                Thread.Sleep(3000); // 3 secs
            }
            for (Balance = 5; Balance > 0; Balance--)
            {
                Thread.Sleep(1000); // 5 secs delay
            }
            if (_shouldStop == AccountState.STOPCLOSED)
                Balance = CLOSED;
        }).Start();
    }

    public void Deposit(int amount) => Balance += amount;

    public bool Withdraw(int amount)
    {
        if (amount > Balance) return false;
        Balance -= amount;
        return true;
    }

    private void applyInterest() => Balance = (Balance * (100 + _interestRate)) / 100;

    public void Close(bool upd) => _shouldStop = upd ? AccountState.STOPCLOSED : AccountState.STOP;

    public bool ThreadFinished(bool sync)
    {
        if (_myThread == null)
            return true;
        if (sync)
        {
            _myThread.Join();
            return true;
        }
        bool t = !_myThread.IsAlive;
        return t;
    }
}

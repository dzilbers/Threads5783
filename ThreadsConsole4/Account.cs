using System.Runtime.CompilerServices;

class Account
{
    int _balance;
    readonly int _interestRate; // integer % number

    private Thread? _myThread = null;
    volatile bool _shouldStop;
    
    public Account(int initBalance, int interest)
    {
        _balance = initBalance;
        _interestRate = interest;
        new Thread(() =>
        {
            _myThread = Thread.CurrentThread;
            _shouldStop = false;
            while (!_shouldStop)
            {
                applyInterest();
                Thread.Sleep(3000); // 3 seconds
            }
            Thread.Sleep(5000);  // 5 seconds delay
        }).Start();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Deposit(int amount)
    {
        timeOutput();
        out1("Deposit");
        _balance += amount;
        out2();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool Withdraw(int amount)
    {
        timeOutput();
        if (amount > _balance)
        {
            out1("No withdraw");
            out2();
            return false;
        }
        out1("Withdraw");
        _balance -= amount;
        out2();
        return true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void applyInterest()
    {
        timeOutput();
        out1("applyInterest");
        _balance = (_balance * (100 + _interestRate)) / 100;
        out2();
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Close()
    {
        // NEVER ABORT A THREAD LIKE THIS: myThread.Abort(); // IT IS DANGEROUS
        timeOutput();
        Console.WriteLine("close: trying");
        _shouldStop = true;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public bool ThreadFinished(bool sync)
    {
        timeOutput();
        if (_myThread == null)
        {
            Console.WriteLine("threadFinished: no thread");
            return true;
        }
        if (sync)
        {
            Console.WriteLine("threadFinished: joining");
            _myThread.Join();
            timeOutput();
            Console.WriteLine("threadFinished: true");
            return true;
        }
        bool t = !_myThread.IsAlive;
        Console.WriteLine("threadFinished: {0}", t);
        return t;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    static void timeOutput() => Console.Write("{0}: ", DateTime.Now.ToString("HH:mm:ss"));
    [MethodImpl(MethodImplOptions.Synchronized)]
    void out1(string loc) => Console.Write("{0}: old balance = {1}, ", loc, _balance);
    [MethodImpl(MethodImplOptions.Synchronized)]
    void out2() => Console.WriteLine("new balance = {0}, ", _balance);
}

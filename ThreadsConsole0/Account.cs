﻿class Account
{
    int _balance;
    readonly int _interestRate; // integer % number

    public Account(int initBalance, int interestRate)
    {
        _balance = initBalance;
        this._interestRate = interestRate;
    }

    public void Deposit(int amount)
    {
        timeOutput(); out1("Deposit");
        _balance += amount;
        out2();
    }

    public bool Withdraw(int amount)
    {
        timeOutput();
        if (amount > _balance)
        {
            out1("No withdraw"); out2();
            return false;
        }
        out1("Withdraw");
        _balance -= amount;
        out2();
        return true;
    }

    void applyInterest()
    {
        timeOutput(); out1("applyInterest");
        _balance = _balance * (100 + _interestRate) / 100;
        out2();
    }

    internal void InterestLoop()
    {
        while (true)
        {
            applyInterest();
            Thread.Sleep(3000); // 3000 milliseconds
        }
    }

    static void timeOutput() => Console.Write("{0}: ", DateTime.Now.ToString("HH:mm:ss"));
    void out1(string loc) => Console.Write("{0}: old balance = {1}, ", loc, _balance);
    void out2() => Console.WriteLine("new balance = {0}, ", _balance);
}

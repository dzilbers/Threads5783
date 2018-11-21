using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ThreadsConsole0
{
    class Account
    {
        private int balance;
        private readonly int interestRate; // integer % number
        public Account(int initBalance, int interestRate)
        {
            this.balance = initBalance;
            this.interestRate = interestRate;
        }

        public void Deposit(int amount)
        {
            timeOutput();
            out1("Deposit");
            balance += amount;
            out2();
        }

        public bool Withdraw(int amount)
        {
            timeOutput();
            if (amount > balance)
            {
                out1("No withdraw");
                out2();
                return false;
            }
            out1("Withdraw");
            balance -= amount;
            out2();
            return true;
        }

        private void applyInterest()
        {
            timeOutput();
            out1("applyInterest");
            balance = (balance * (100 + interestRate)) / 100;
            out2();
        }

        public void interestLoop()
        {
            while (true)
            {
                applyInterest();
                Thread.Sleep(3000); // 3000 milliseconds
            }
        }

        private void timeOutput()
        {
            Console.Write("{0}: ", DateTime.Now.ToString("HH:mm:ss"));
        }
        private void out1(string loc)
        {
            Console.Write("{0}: old balance = {1}, ", loc, balance);
        }
        private void out2()
        {
            Console.WriteLine("new balance = {0}, ", balance);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Account myAccount = new Account(1000, 2);
            myAccount.interestLoop();
        }
    }
}

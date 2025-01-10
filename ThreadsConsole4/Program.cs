Random rand = new();

Account myAccount = new(1000, 2);

while (!myAccount.ThreadFinished(false))
{
    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
    switch (keyInfo.KeyChar)
    {
        case '1':
            myAccount.Deposit(rand.Next(100));
            break;
        case '2':
            myAccount.Withdraw(rand.Next(200));
            break;
        case '0':
            myAccount.Close();
            myAccount.ThreadFinished(true);
            Thread.Sleep(200);
            break;
    }
}

Random rand = new();

Account myAccount = new(1000, 2);

bool finishing = false;
while (!myAccount.ThreadFinished(false))
{
    if (finishing)
    {
        Thread.Sleep(1000);
        continue;
    }
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
            //finishing = true;
            myAccount.ThreadFinished(true); 
            break;
    }
    
}

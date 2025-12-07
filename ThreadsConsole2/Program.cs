_ = new Account(1000, 2);
Thread.Sleep(300);
_ = new Account(200, 1);
Thread.Sleep(300);
_ = new Account(3000, 2);
Thread.Sleep(300);
_ = new Account(10000, 3);

while (true)
{
    Thread.Sleep(1000);
    Console.WriteLine("Main is alive");
}

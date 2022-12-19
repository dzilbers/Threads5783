#pragma warning disable CA1806 // Do not ignore method results
new Account(1000, 2);
Thread.Sleep(300);
new Account(200, 1);
Thread.Sleep(300);
new Account(3000, 2);
Thread.Sleep(300);
new Account(10000, 3);

while (true)
{
    Thread.Sleep(1000);
    Console.WriteLine("Main is alive");
}

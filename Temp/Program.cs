// See https://aka.ms/new-console-template for more information
Thread me = Thread.CurrentThread;
new Thread(() => { Thread.Sleep(1000); me!.Interrupt(); Console.WriteLine($"Sent Interrupt to {me.ManagedThreadId}"); }).Start();
Console.WriteLine($"Going to sleep {me.ManagedThreadId}");
try { Thread.Sleep(5000); }
catch (Exception ex) { Console.WriteLine($"Caught {ex}"); }
Console.WriteLine("Done");

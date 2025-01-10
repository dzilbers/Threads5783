namespace TaskTrials;

class Program
{
    private static async Task Main()
    {
        Console.WriteLine($"Start: {Environment.CurrentManagedThreadId}"); // 1
        var t = func();
        Console.WriteLine("After func call");
        Thread.Sleep(50);
        Console.WriteLine("After delay 0.05s");
        Thread.Sleep(100);
        Console.WriteLine("After delay 0.1s");
        await t;
        Console.WriteLine("After await");
        Thread.Sleep(200);
        Console.WriteLine("After delay 0.2s");
    }

    static async Task func()
    // without async (with Sleep and return) - the function is called synchronously
    {
        Console.WriteLine($"func: {Environment.CurrentManagedThreadId}"); // 1
        await Task.Delay(100);
        //Thread.Sleep(100);
        Console.WriteLine($"func: after sleep/delay");
        Console.WriteLine($"func: {Environment.CurrentManagedThreadId}"); // 4
        await Task.Delay(100);
        Console.WriteLine($"func: after sleep/delay");
        Console.WriteLine($"func: {Environment.CurrentManagedThreadId}"); // 4
        //Thread.Sleep(100);
        //return Task.CompletedTask;
    }
}

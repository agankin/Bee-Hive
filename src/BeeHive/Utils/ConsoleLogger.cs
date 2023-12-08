namespace BeeHive;

internal class ConsoleLogger
{
    public static void Log(string message)
    {
        var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss:FFF");
        var threadId = Thread.CurrentThread.ManagedThreadId;

        Console.WriteLine($"{time} - {threadId}: {message}");
    }
}
using System.Diagnostics;

namespace BeeHive;

internal class DebugLogger
{
    [Conditional("DEBUG")]
    public static void LogDebug(string message)
    {
        var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss:FFF");
        var threadId = Thread.CurrentThread.ManagedThreadId;

        Console.WriteLine($"{time} - {threadId}: {message}");
    }
}
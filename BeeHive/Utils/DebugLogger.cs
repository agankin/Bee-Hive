namespace BeeHive
{
    public class DebugLogger
    {
        public static void Log(string message) =>
            Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss:FFF} - Thread {Thread.CurrentThread.ManagedThreadId}: {message}");
    }
}
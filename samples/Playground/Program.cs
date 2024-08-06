using BeeHive;

var hive = new HiveBuilder()
    .WithMinLiveThreads(3)
    .WithMaxLiveThreads(5)
    .WithThreadIdleBeforeStop(3000)
    .Build();
hive.Run();

var computeQueue = hive.GetQueueFor<int, int>(Square);

var computations = QueueComputations(computeQueue);
await Task.WhenAll(computations);

Console.ReadKey(true);

IEnumerable<Task> QueueComputations(HiveQueue<int, int> computeQueue)
{
    for (var i = 0; i < 6; i++)
        yield return computeQueue.Compute(i).Task
            .ContinueWith(task => 
            {
                Log($"Result: {task.Result}");
                return 0;
            });
}

async ValueTask<int> Square(int number)
{
    Log($"Before sleep and computing square of {number}...");
    Thread.Sleep(1000);
    await Task.Delay(1000);

    Log($"After sleep next computing square of {number}...");
    return number * number;
}

void Log(string message)
{
    var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss:FFF");
    var threadId = Thread.CurrentThread.ManagedThreadId;

    Console.WriteLine($"{time} - {threadId}: {message}");
}
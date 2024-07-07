using BeeHive;

var computeSquaresHive = Hive.ToCompute<int, int>(Square)
    .WithMinLiveThreads(3)
    .WithMaxLiveThreads(5)
    .WithThreadIdleBeforeStop(3000)
    .Build();

computeSquaresHive.Run();

var computations = QueueComputations(computeSquaresHive);
await Task.WhenAll(computations);

Console.ReadKey(true);

IEnumerable<Task> QueueComputations(Hive<int, int> computeSquaresHive)
{
    for (var i = 0; i < 30; i++)
        yield return computeSquaresHive.Compute(i).Task
            .ContinueWith(task => task.Result.Map(value => 
            {
                Log($"Result: {value}");
                return 0;
            }));
}

async ValueTask<int> Square(int number)
{
    Log($"Before sleep and computing square of {number}...");
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
using BeeHive;

var hive = new Hive();

var computeSquares = hive.AddComputation<int, int>(
    Square,
    config => config
        .MinLiveThreads(3)
        .MaxLiveThreads(5)
        .ThreadWaitForNext(3000));

var computations = QueueComputations(computeSquares);
await Task.WhenAll(computations);

Console.ReadKey(true);

IEnumerable<Task> QueueComputations(HiveComputation<int, int> computeSquares)
{
    for (var i = 0; i < 30; i++)
        yield return computeSquares.Compute(i)
            .ContinueWith(task => task.Result.Map(value => 
            {
                Log($"Result: {value}");
                return 0;
            }));
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
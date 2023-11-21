using BeeHive;

var hive = new Hive();

var computeSquares = hive.AddComputation<int, int>(
    Square,
    config => config
        .MinLiveThreads(3)
        .MaxLiveThreads(5));

var computations = QueueComputations(computeSquares);
await Task.WhenAll(computations);

Console.Write("Computations finished! To exit press any key...");
Console.ReadKey(true);

IEnumerable<Task> QueueComputations(HiveComputation<int, int> computeSquares)
{
    for (var i = 0; i < 30; i++)
        yield return computeSquares.Compute(i).ContinueWith(task => Log($"Result: {task.Result}"));
}

int Square(int number)
{
    Thread.Sleep(1000);
    return number * number;
}

void Log(string message) =>
    Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss:FFF}: {message}");
using BeeHive;

var hive = new Hive();

var squareTask = hive.AddComputation<int, int>(
    Square,
    config => config
        .MinLiveThreads(3)
        .MaxLiveThreads(5));

for (var i = 0; i < 10; i++)
    squareTask.QueueRequest(i);

var results = squareTask.GetNewResultCollection();

foreach (var result in results.GetConsumingEnumerable())
    Log($"Result: {result}");

int Square(int number)
{
    Thread.Sleep(1000);
    return number * number;
}

void Log(string message) =>
    Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss:FFF}: {message}");
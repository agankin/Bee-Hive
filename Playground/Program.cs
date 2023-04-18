using BeeHive;

var hive = new Hive();

var squareTask = hive.AddComputation<int, int>(
    Square,
    config => config.MaxParallelExecution(3).WithMinLoadScheduling());

squareTask.QueueRequest(10);
squareTask.QueueRequest(11);
squareTask.QueueRequest(12);
squareTask.QueueRequest(13);
squareTask.QueueRequest(14);
squareTask.QueueRequest(15);

Log("Getting result collection...");
var results = squareTask.GetNewResultCollection();
Log("Got result collection");

foreach (var result in results.GetConsumingEnumerable())
    Log($"Result: {result}");

int Square(int number)
{
    Log("Computation start");
    Thread.Sleep(1000);
    Log("Computation end");
    return number * number;
}

void Log(string message) => Console.WriteLine($"{DateTime.Now:dd.MM.yyyy HH:mm:ss:FFF} - Thread {Thread.CurrentThread.ManagedThreadId}: {message}");
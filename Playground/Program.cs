using BeeHive;

var hive = new Hive();

var squareTask = hive.AddComputation<int, int>(
    Square,
    config => config
        .MinLiveThreads(3)
        .MaxParallelExecution(5)
        .WithMinLoadScheduling());

squareTask.QueueRequest(10);
squareTask.QueueRequest(11);
squareTask.QueueRequest(12);
squareTask.QueueRequest(13);
squareTask.QueueRequest(14);
squareTask.QueueRequest(15);
squareTask.QueueRequest(16);
squareTask.QueueRequest(17);
squareTask.QueueRequest(18);
squareTask.QueueRequest(19);
squareTask.QueueRequest(20);

var results = squareTask.GetNewResultCollection();

foreach (var result in results.GetConsumingEnumerable())
    DebugLogger.Log($"Result: {result}");

int Square(int number)
{
    Thread.Sleep(1000);
    return number * number;
}
using BeeHive;
using static BeeHive.Samples.ComputationFunctions;

Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)
    .WithMaxLiveThreads(4)
    .WithThreadIdleBeforeStop(milliseconds: 1000)
    .Build();

hive.Run();

var isPrimeQueue = hive.GetQueueFor<string, bool>(IsPrimeNumber);
using var isPrimeResults = isPrimeQueue.CreateResultBag();

using var cts = new CancellationTokenSource();

_ = isPrimeQueue.EnqueueCompute("1000000007");
_ = isPrimeQueue.EnqueueCompute("1000000009");
_ = isPrimeQueue.EnqueueCompute("1000000011");
_ = isPrimeQueue.EnqueueCompute("1000000021");

await isPrimeQueue.WhenAll();

while (isPrimeResults.TryTake(out var result))
{
    var resultDescription = result.State switch
    {
        ResultState.Success => $"IsPrime(\"{result.Request}\") => {result.Value}",
        ResultState.Error => $"IsPrime(\"{result.Request}\") => Error({result.Error?.Message})",
        ResultState.Cancelled => $"IsPrime(\"{result.Request}\") => Cancelled",
        _ => throw new Exception($"Unsupported {nameof(ResultState)} value: {result.State}.")
    };

    Console.WriteLine(resultDescription);
}

Console.ReadKey(true);
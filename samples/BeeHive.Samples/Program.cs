﻿using System.Reactive.Linq;
using BeeHive;

using static BeeHive.Samples.ComputationFunctions;

Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)
    .WithMaxLiveThreads(4)
    .WithThreadIdleBeforeStop(milliseconds: 1000)
    .Build();

hive.Run();

var isPrimeQueue = hive.CreateQueueFor<long, bool>(IsPrimeNumber);
using var isPrimeResults = isPrimeQueue.CreateResultBag();

using var cts = new CancellationTokenSource();

_ = isPrimeQueue.AddRequest(1000000007);
_ = isPrimeQueue.AddRequest(1000000009);
_ = isPrimeQueue.AddRequest(1000000011);
_ = isPrimeQueue.AddRequest(1000000021);

await isPrimeQueue.WhenAll();

while (isPrimeResults.TryTake(out var result))
    Console.WriteLine(Format(result));

using var subscription = isPrimeQueue.Subscribe(
    onNext: result => result.Match(
        onValue: value => Console.WriteLine($"Result: {value}"),
        onError: error => Console.WriteLine($"Error occured: {error.Message}"),
        onCancelled: () => Console.WriteLine($"Computation was cancelled")
    )
);

_ = isPrimeQueue.AddRequest(1000000033);
_ = isPrimeQueue.AddRequest(1000000037);
_ = isPrimeQueue.AddRequest(1000000087);
_ = isPrimeQueue.AddRequest(1000000091);
_ = isPrimeQueue.AddRequest(1000000093);

Console.ReadKey(true);

string Format(Result<long, bool> result) => result.State switch
{
    ResultState.Success => $"IsPrime({result.Request}) => {result.Value}",
    ResultState.Error => $"IsPrime({result.Request}) => Error({result.Error?.Message})",
    ResultState.Cancelled => $"IsPrime({result.Request}) => Cancelled",
    _ => throw new Exception($"Unsupported {nameof(ResultState)} value: {result.State}.")
};
# Bee Hive

![NuGet Version](https://img.shields.io/nuget/v/Bee-Hive)

A dedicated Thread Pool for computations parallelization.

It is useful for running long CPU intensive computations in background without risk of the standard .NET Thread Pool starvation.

## Features

- Provides explicit strongly typed queues for computation requests.
- Represents queued computations as Hive Tasks that can be awaited/cooperatively cancelled.
- Supports synchronous and asynchronous computations.
- Exposes Hive Queues as observables sending reactive notifications about completed results.
- Supports accumulating computed results into Result Bags.
- Threads can be dynamically added for extra load and automaticly stopped after some idle time.
- Has configurable lower/upper number of running threads and idle time for threads to be stopped.

## Quick start

- [Building a Hive](#building-a-hive)
- [Requesting computations](#requesting-computations)
- [Hive Task properties](#hive-task-properties)
- [Cancelling computations](#cancelling-computations)
- [Error handling](#error-handling)
- [Observing results](#observing-results)
- [Accumulating results in Result Bags](#accumulating-results-in-result-bags)
- [Disposing a Hive](#disposing-a-hive)
- [Functions used in examples](#functions-used-in-examples)

### Building a Hive

Hive instances can be created using HiveBuilder:

```cs
Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)                                // Sets minimal number of threads in the Hive.
    .WithMaxLiveThreads(4)                                // Sets maximal number of threads in the Hive.
    .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of idle time for threads to be stopped.
    .Build();                                             // Builds the Hive.

hive.Run();                                               // Runs the Hive. This starts minimal number of threads.
```

### Requesting computations

The first step is obtaining a Hive Queue. Hive Queues are collections holding requests for computations. Each Hive can have unlimited number of Hive Queues. It fetches requests in FIFO order of adding from all of its queues and performs computations within its single Thread Pool.

The sample below shows how to create different instances of Hive Queues. The definition of computational functions used can be found in [Functions used in examples](#functions-used-in-examples) section.

```cs
// A queue to calculate is a number prime or not.
HiveQueue<long, bool> isPrimeQueue = hive.CreateQueueFor<long, bool>(IsPrimeNumber); 

// A queue to calculate square root of an integer.
HiveQueue<int, double> sqrtQueue = hive.CreateQueueFor<int, double>(SqrtAsync);

// Another queue of the previous type.
HiveQueue<int, double> sqrtQueue2 = hive.CreateQueueFor<int, double>(SqrtAsync);
```

Computations are called by adding requests to Hive Queues. In response they return Hive Tasks representing computations that will be performed at some moment in future.

```cs
// Requesting 3 computations.
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1007); // Returns an instance of Hive Task.
_ = isPrimeQueue.AddRequest(2333);
_ = isPrimeQueue.AddRequest(5623);

// Requesting 2 computations in other queue.
_ = sqrtQueue.AddRequest(121);
_ = sqrtQueue.AddRequest(144);
```

Hive Queues support enumeration of pending or currently run tasks. Once a Hive Task successfully completes, fails with an error or becomes cancelled it is removed from the owning Hive Queue.

```cs
foreach (HiveTask<long, bool> hiveTask in isPrimeQueue)
    Console.WriteLine($"Computing is {hiveTask.Request} prime number or not...");
```

It prints lines:

```
Computing is 1007 prime number or not...
Computing is 2333 prime number or not...
Computing is 5623 prime number or not...
```

### Hive Task properties

Each Hive Task has properties containing the initial computation request, the current state and optionally a computed result:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1000000009);

await hiveTask;

Debug.Assert(hiveTask.Request == 1000000009);                        // A request the computation was invoked for.
Debug.Assert(hiveTask.State == HiveTaskState.SuccessfullyCompleted); // After awaiting the state is "successfully completed".
Debug.Assert(hiveTask.Result?.Value == true);                        // After completion the result contains a computed value.
```

A canonical Task can be obtained via property or by implicit conversion:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(256);

Task<double> task = hiveTask.Task;          // Obtaining Task from property.
Task<double> theSameTask = hiveTask;        // Obtaining Task by implicit conversion.
```

Hive Tasks are awaitables:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1000000007);
bool isPrime = await hiveTask;
```

### Cancelling computations

Pending or running computations can be cancelled.

Cancellation is guranteed when a Hive Task is in pending state. Cancellation time depends on how the corresponding computation delegate supports cooperative cancellation. If the delegate doesn't support cancellation then the computation will be cancelled only after the delegate execution completes or an error occures.

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(64);
hiveTask.Cancel();

try
{
    await hiveTask;                                                   // Awaits for cancellation.
}
catch (TaskCanceledException) {}
Debug.Assert(hiveTask.State == HiveTaskState.Cancelled);              // Now the task is in "cancelled" state.
```

### Error handling

Hive Tasks can go to error state if exceptions occure during execution.

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);         // Requesting unsupported square root computation of a negative number.

try
{
    await hiveTask;                                                 // Awaits and throws Exception.
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);                                  // Prints "Cannot calculate sqrt of the negative value.".
}
Debug.Assert(hiveTask.State == HiveTaskState.Error);                // Now the task is in error state.
```

A handy extension method exists to await safely without throwing exceptions. It returns an instance of Result&lt;TRequest, TResult&gt;.

Result represents an one of the three states:

- A computation result
- An error occured
- Being in the cancelled state

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);          // Requesting unsupported square root of a negative number.
Result<int, double> result = await hiveTask.AsyncResult();

// The code below matches the result and prints: "Error: Cannot calculate sqrt of the negative value.".
result.Match(
    onValue: value => Console.WriteLine($"Result: {value}"),
    onError: error => Console.WriteLine($"Error occured: {error.Message}"),
    onCancelled: () => Console.WriteLine($"Computation was cancelled")
);
```

Another extension method exists to safely await all tasks in a Hive Queue:

```cs
await isPrimeQueue.WhenAll();        // After this line all requested computations are completed.
```

### Observing results

Hive Queues are observables notifying about results of computations.

```cs
using System.Reactive.Linq;

// Subscribes for results of finished computations.
using var subscription = isPrimeQueue.Subscribe(
    onNext: result => result.Match(
        onValue: value => Console.WriteLine($"Result: {value}"),
        onError: error => Console.WriteLine($"Error occured: {error.Message}"),
        onCancelled: () => Console.WriteLine($"Computation was cancelled")
    )
);

isPrimeQueue.AddRequest(5779);
isPrimeQueue.AddRequest(5781);
isPrimeQueue.AddRequest(5783);
```

### Accumulating results in Result Bags

A Result Bag is a collection created for a Hive Queue and automatically filled with results of finished computations.

It has methods for taking out results:

- TryTake - tries to get a result without waiting and returns a flag meaning if a result is found.
- TryTakeOrWait - tries to get a result and returns immediately if a result exists or waits for some time/infinitely for a new result. Returns a flag meaning if a result is found.

When a Result Bag is no longer needed it must be disposed to prevent further filling with new results.

```cs
// Creating a result bag.
using IHiveResultBag<int, double> resultBag = sqrtQueue.CreateResultBag();

// Requesting some computations.
_ = sqrtQueue.AddRequest(121);
_ = sqrtQueue.AddRequest(144);
_ = sqrtQueue.AddRequest(256);

await sqrtQueue.WhenAll();                          // Awaits for all computations to complete.

// Taking and displaying all items from the result bag.
while (resultBag.TryTake(out var result))
{
    Console.WriteLine($"Sqrt of {result.Request}: State={result.State}, Value={result.Value}, Error={result.Error?.Message}");
}
```

It prints the following lines:

```W
Sqrt of 144: State=Success, Value=12, Error=
Sqrt of 256: State=Success, Value=16, Error=
Sqrt of 121: State=Success, Value=11, Error=
```

Another example of taking out with waiting:

```cs
// Requests some additional computations.
_ = sqrtQueue.AddRequest(289);
_ = sqrtQueue.AddRequest(-625);

// Waiting for each next result up to 5000ms and displaying it.
while (resultBag.TryTakeOrWait(5000, out var result))
{
    Console.WriteLine($"Sqrt of {result.Request}: State={result.State}, Value={result.Value}, Error={result.Error?.Message}");
}
```

It prints the following lines:

```
Sqrt of 289: State=Success, Value=17, Error=
Sqrt of -625: State=Error, Value=0, Error=Cannot calculate sqrt of the negative value.
```

### Disposing a Hive

When a Hive is no longer needed it must be disposed.

On disposal all running computations receive cancellation. Idle threads are stopped immediately but busy threads continue running until their currently run computations get cancelled or complete/fail.

```cs
var hive1 = new HiveBuilder().Build();
hive1.Dispose();                           // Returns without blocking. The Hive's busy threads finish at some moment in future.

var hive2 = new HiveBuilder().Build();
await hive2.DisposeAsync();                // Awaits all threads to finish.
```

### Functions used in examples

```cs
/// <summary>
/// A sync function determining if a number is prime.
/// The implementation is inefficient but good as an example of a long running function.
/// </summary>
public static bool IsPrimeNumber(long number, CancellationToken cancellationToken)
{
    Thread.Sleep(100);                                    // Additionally simulates some work.
    cancellationToken.ThrowIfCancellationRequested();

    if (number < 0)
        throw new Exception("The number must be greater than or equal to zero.");

    if (number == 2)
        return true;
    
    if (number == 0 || number == 1 || number % 2 == 0)
        return false;

    var divisor = 3;
    while (divisor <= number / 2)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (number % divisor++++ == 0)
            return false;
    }
    
    return true;
}

/// <summary>
/// An async function computing square root of an integer number.
/// </summary>
public static async Task<double> SqrtAsync(int value, CancellationToken cancellationToken)
{
    // Simulates long work for 1 second.
    for (var i = 0; i < 10; i++)
    {
        await Task.Delay(100);
        cancellationToken.ThrowIfCancellationRequested();
    }

    if (value < 0)
        throw new Exception("Cannot calculate sqrt of the negative value.");

    var result = Math.Sqrt(value);
    return result;
}
```
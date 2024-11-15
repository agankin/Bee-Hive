# Bee Hive

![NuGet Version](https://img.shields.io/nuget/v/Bee-Hive)

A dedicated Thread Pool for computations parallelization.

It is useful for running long CPU intensive computations in background without risk of the standard .NET Thread Pool starvation.

## Features

- Provides explicit strongly typed queues for requested computations.
- Queued computations are represented by Hive Tasks that can be awaited/cooperatively cancelled.
- Supports synchronous and asynchronous computations.
- Supports accumulating results of completed computations into Result Bags.
- Threads can be dynamically added for extra load and automaticly stopped after some idle time.
- Has configurable lower/upper number of running threads and idle time for threads to be stopped.

## Quick start

- [Building a Hive](#building-a-hive)
- [Creating Hive Queues](#creating-hive-queues)
- [Requesting computations](#requesting-computations)
- [Working with Hive Tasks](#working-with-hive-tasks)
- [Cancelling computations](#cancelling-computations)
- [Handling errors](#handling-errors)
- [Accumulating results in Result Bags](#accumulating-results-in-result-bags)
- [Disposing Hive](#disposing-hive)
- [Functions used in examples](#functions-used-in-examples)

### Building a Hive

Hive instances are configured and built using HiveBuilder:

```cs
Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)                                // Sets minimal number of threads in the Hive.
    .WithMaxLiveThreads(4)                                // Sets maximal number of threads in the Hive.
    .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of idle time for threads to be stopped.
    .Build();                                             // Builds the Hive.

hive.Run();                                               // Runs the Hive. This starts minimal number of threads.
```

### Creating Hive Queues

Hive Queue is a queue created for a computation delegate and holding requests for the computation. Computation delegate takes request and optionally cancellation token as input and returns a plain result or a Task/ValueTask representing result of an async computation.

Each Hive can have unlimited number of Hive Queues. Hive fetches requests in the order of adding them to all its Hive Queues and performs computations within its single Thread Pool.

The definition of functions used in examples can be found in [Functions used in examples](#functions-used-in-examples) section.

```cs
// A queue to calculate is a number prime or not.
HiveQueue<long, bool> isPrimeQueue = hive.GetQueueFor<long, bool>(IsPrimeNumber); 

// A queue to calculate square root of an integer.
HiveQueue<int, double> sqrtQueue = hive.GetQueueFor<int, double>(SqrtAsync);

// Separate queue of the same type.
HiveQueue<int, double> sqrtQueue2 = hive.GetQueueFor<int, double>(SqrtAsync);
```

### Requesting computations

Computational requests are added to Hive Queues to perform computations. Requested computations are represented by Hive Task instances.

```cs
// Requesting a few computations.
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1007); // The call returns an instance of Hive Task.
_ = isPrimeQueue.AddRequest(2333);
_ = isPrimeQueue.AddRequest(5623);
_ = isPrimeQueue.AddRequest(7753);
_ = isPrimeQueue.AddRequest(7761);

_ = sqrtQueue.AddRequest(121);
_ = sqrtQueue.AddRequest(144);
```

### Working with Hive Tasks

Hive Queue supports enumeration of pending or currently run Hive Tasks. Once a Hive Task completes or fails with an error it gets removed from the owning Hive Queue.

```cs
foreach (HiveTask<long, bool> hiveTask in isPrimeQueue)
    Console.WriteLine($"Computing is {hiveTask.Request} prime or not. State={hiveTask.State}.");

Console.WriteLine();

foreach (HiveTask<int, double> hiveTask in sqrtQueue)
    Console.WriteLine($"Computing square root of {hiveTask.Request}. State={hiveTask.State}.");
```

It prints something like:

```
Computing is 1007 prime or not. State=InProgress.
Computing is 2333 prime or not. State=InProgress.
Computing is 5623 prime or not. State=InProgress.
Computing is 7753 prime or not. State=InProgress.
Computing is 7761 prime or not. State=Pending.

Computing square root of 121. State=Pending.
Computing square root of 144. State=Pending.
```

Hive Task is awaitable:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1000000007);
bool isPrime = await hiveTask;
```

An extension method exists for safely awaiting without exceptions thrown on error:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);          // Unsupported square root of negative number.
Result<int, double> result = await hiveTask.AsyncResult();

// Matching possible states of the computation result.
// Prints "Cannot calculate sqrt of negative value.".
result.Match(
    onValue: value => Console.WriteLine($"Value: {value}"),
    onError: error => Console.WriteLine($"Error: {error.Message}"),
    onCancelled: () => Console.WriteLine($"Cancelled")
);
```

One more handy extension method exists allowing to await all Hive Tasks in a Hive Queue:

```cs
await isPrimeQueue.WhenAll();        // After this line all IsPrimeNumber computations are completed.
```

Canonical Task can be obtained via property or by implicit conversion:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(256);

Task<double> task = hiveTask.Task;          // Obtaining Task from property.
Task<double> theSameTask = hiveTask;        // Obtaining Task by implicit conversion.
```

Hive Task has properties containing initial computation request, current state and computed result:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest(1000000009);

await hiveTask;

Debug.Assert(hiveTask.Request == 1000000009);                        // Request the computation was invoked for.
Debug.Assert(hiveTask.State == HiveTaskState.SuccessfullyCompleted); // After awaiting state becomes SuccessfullyCompleted.
Debug.Assert(hiveTask.Result?.Value == true);                        // After completion result contains computed value.
```

### Cancelling computations

Pending or running computations can be cancelled by invoking **bool Cancel()** Hive Task instance method.

Cancellation is guranteed when a Hive Task is in Pending state. For a running computation the cancellation time depends on how the computation delegate supports cooperative cancellation. If it doesn't support it at all then cancellation will happen only after the delegate execution completes or error occures.

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(64);
hiveTask.Cancel();

try
{
    await hiveTask;                                                   // Await for cancellation.
}
catch (TaskCanceledException) {}
Debug.Assert(hiveTask.State == HiveTaskState.Cancelled);              // Now the task is in Cancelled state.
```

### Handling errors

If an exception occures during a computation delegate execution the corresponding Hive Task will go to Error state.

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);         // Unsupported square root of negative number.

try
{
    await hiveTask;                                                 // Await for fail.
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);                                  // Prints "Cannot calculate sqrt of negative value.".
}
Debug.Assert(hiveTask.State == HiveTaskState.Error);                // Now the task is in Error state.
```

### Accumulating results in Result Bags

Result Bag is a collection created for a Hive Queue and filled with results of completed computations.

Result Bag is an object returned from **IHiveResultBag<int, double>** . It supports enumeration and has overloaded methods for taking out results:

- **TryTake** - tries to get a result without waiting and returns a flag meaning if a result is found.
- **TryTakeOrWait** - tries to get a result and returns immediately if a result exists or waits for some time/infinitely for a new result. Returns a flag meaning if a result is found.

When a Result Bag is no longer needed it must be disposed to prevent further filling with new results.

```cs
using IHiveResultBag<int, double> resultBag = sqrtQueue.CreateResultBag();

// Request some computations.
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

It prints something like this: 

```cs
// ***************** CONSOLE *******************
// Sqrt of 144: State=Success, Value=12, Error=
// Sqrt of 256: State=Success, Value=16, Error=
// Sqrt of 121: State=Success, Value=11, Error=
// *********************************************
```

```cs
// Request some additional computations.
_ = sqrtQueue.AddRequest(289);
_ = sqrtQueue.AddRequest(-625);

// Waiting for each next result up to 5000ms and displaying it.
while (resultBag.TryTakeOrWait(5000, out var result))
{
    Console.WriteLine($"Sqrt of {result.Request}: State={result.State}, Value={result.Value}, Error={result.Error?.Message}");
}

// ***************** CONSOLE *******************
// Sqrt of 289: State=Success, Value=17, Error=
// Sqrt of -625: State=Error, Value=0, Error=Cannot calculate sqrt of negative value.
// *********************************************
```

### Disposing Hive

Hive must be disposed when it is no longer needed.

On disposal all running computations receive cancellation. Idle threads are stopped immediately but busy threads continue running until their computations cancel/complete.

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
/// The implementation is inefficient but good as an example of long running function.
/// </summary>
public static bool IsPrimeNumber(long number, CancellationToken cancellationToken)
{
    Thread.Sleep(100);                                    // Additionally simulates some work.
    cancellationToken.ThrowIfCancellationRequested();

    if (number < 0)
        throw new Exception("Number must be greater than or equal to zero.");

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
        throw new Exception("Cannot calculate sqrt of negative value.");

    var result = Math.Sqrt(value);
    return result;
}
```
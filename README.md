# Bee Hive

![NuGet Version](https://img.shields.io/nuget/v/Bee-Hive)

A dedicated Thread Pool for computations parallelization.

It is useful for running long CPU intensive computations without risk of standard .NET Thread Pool starvation.

## Features

- Computations are scheduled for execution in explicit strongly typed Queues.
- Queued computations are represented by Hive Tasks that can be awaited/cooperatively cancelled.
- Supports synchronous and asynchronous computations.
- Can have any number of Queues.
- Supports accumulating results of completed computations into Result Bags.
- Threads can be dynamically added for extra load and automaticly stopped after some idle time.
- Has configurable lower/upper number of running threads and idle time for threads to be stopped.

## Quick start

- [Building a Hive](#building-a-hive)
- [Hive Queues](#hive-queues)
    - [Enqueueing computations](#enqueueing-computations)
    - [Enumerating computations](#enumerating-computations)
    - [Awaiting Hive Queue](#awaiting-hive-queue)
- [Hive Tasks](#hive-tasks)
    - [Accessing Hive Tasks](#accessing-hive-tasks)
    - [Hive Tasks properties](#hive-tasks-properties)
    - [Hive Tasks cancellation](#hive-tasks-cancellation)
    - [Hive Tasks errors](#hive-tasks-errors)
- [Result Bags](#result-bags)
- [Hive disposal](#hive-disposal)
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

### Hive Queues

Hive runs computations fetching them from Hive Queues. Computations from all Queues are executed within the single Hive's thread pool.

#### Enqueueing computations

The example below shows how Hive Queues are created and computations are added:

```cs
HiveQueue<string, bool> isPrimeQueue = hive.GetQueueFor<string, bool>(IsPrimeNumber);
HiveQueue<int, double> sqrtQueue = hive.GetQueueFor<int, double>(SqrtAsync);

HiveTask<string, bool> isPrimeHiveTask = isPrimeQueue.EnqueueCompute("1007"); // The call returns an instance of Hive Task.
_ = isPrimeQueue.EnqueueCompute("2333");
_ = isPrimeQueue.EnqueueCompute("5623");
_ = isPrimeQueue.EnqueueCompute("7753");

_ = sqrtQueue.EnqueueCompute(121);
_ = sqrtQueue.EnqueueCompute(144);
```

#### Enumerating computations

Enqueued computations are represented by instances of HiveTask&lt;TRequest, TResult&gt;. Queues implement IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt; and allow enumeration of queued Hive Tasks. Queues contain only pending or currently executed Tasks. Once a Hive Task completes it gets removed from the owning Queue.

The example of Hive Tasks enumeration:

```cs
foreach (HiveTask<string, bool> hiveTask in isPrimeQueue)
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

Computing square root of 121. State=Pending.
Computing square root of 144. State=Pending.
```

#### Awaiting Hive Queue

A handy extension method exists allowing to await all computations in a Queue:

```cs
await isPrimeQueue.WhenAll();        // After this line all IsPrimeNumber computations are completed.
```

### Hive Tasks

A Hive Task represents a computation that will be performed by the Hive.

#### Accessing Hive Tasks

A Hive Task can be obtained from the return value of HiveQueue&lt;TRequest, TResult&gt;.EnqueueCompute(TRequest request) method:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.EnqueueCompute(64);
```

Also it can be obtained by enumeration/applying LINQ operators to HiveQueue&lt;TRequest, TResult&gt; implementing IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt;:

```cs
sqrtQueue.EnqueueCompute(225);
HiveTask<int, double> hiveTask = sqrtQueue.First();
```

#### Hive Tasks properties

Hive Tasks are awaitables:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.EnqueueCompute("1000000007");
bool isPrime = await hiveTask;
```

Canonical Task&lt;TResult&gt; can be obtained via Task property or by implicit conversion:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.EnqueueCompute(256);

Task<double> task = hiveTask.Task;
Task<double> theSameTask = hiveTask;
```

Hive Task has properties containing initial computation request and current state:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.EnqueueCompute("1000000009");

await hiveTask;

Debug.Assert(hiveTask.Request == "1000000009");                      // Request the computation was invoked for.
Debug.Assert(hiveTask.State == HiveTaskState.SuccessfullyCompleted); // After awaiting state becomes SuccessfullyCompleted.
```

#### Hive Tasks cancellation

If a computation supports cooperative cancellation it can be cancelled with HiveTask&lt;TRequest, TResult&gt;.Cancel() method call:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.EnqueueCompute(64);
hiveTask.Cancel();

try
{
    await hiveTask;                                                   // Await for cancellation.
}
catch (TaskCanceledException) {}
Debug.Assert(hiveTask.State == HiveTaskState.Cancelled);              // Now the Task is in Cancelled state.
```

The call will have no effect if cooperative cancellation is not supported.

#### Hive Tasks errors

A Hive Task can go to an error state if an exception occures during the computation:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.EnqueueCompute(-16);     // Unsupported square root of negative number.

try
{
    await hiveTask;                                                        // Await for fail.
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);                                         // Prints "Cannot calculate sqrt of negative value.".
}
Debug.Assert(hiveTask.State == HiveTaskState.Error);                       // Now the task is in Error state.
```

### Result Bags

Result Bags allow to accumulate results of completed computations. A Result Bag is created for a Queue and receives results of completed computations from that Queue.

Result Bags support enumeration of results and has methods for taking out elements:

- TryTake - tries to get a result without waiting and returns a flag meaning if a result is found.
- TryTakeOrWait - tries to get a result and returns immediately if a result exists or waits for some time/infinitely for a new result. Returns a flag meaning if a result is found.

When a Result Bag is no longer needed it must be disposed to prevent further filling with new coming results.

```cs
using IHiveResultBag<int, double> resultBag = sqrtQueue.CreateResultBag();

// Enqueue some computations.
_ = sqrtQueue.EnqueueCompute(121);
_ = sqrtQueue.EnqueueCompute(144);
_ = sqrtQueue.EnqueueCompute(256);

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
// Enqueue some additional computations.
_ = sqrtQueue.EnqueueCompute(289);
_ = sqrtQueue.EnqueueCompute(-625);

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

### Hive disposal

When a Hive is no longer needed it must be disposed. On disposal all running computations receive cancellation, idle threads are stopped immediately but busy threads continue running until their computations cancel/complete.

```cs
var hive1 = new HiveBuilder().Build();
hive1.Dispose();                           // Returns without blocking. The Hive's busy threads finish at some moment in future.

var hive2 = new HiveBuilder().Build();
await hive2.DisposeAsync();                // Awaits all threads to finish.
```

### Functions used in examples

Definition of used functions in the examples:

```cs
/// <summary>
/// A sync function determining if an arbitrarily large number in string is prime.
/// The implementation is inefficient but good as an example of a long running function.
/// </summary>
public static bool IsPrimeNumber(string numberString, CancellationToken cancellationToken)
{
    Thread.Sleep(100);
    cancellationToken.ThrowIfCancellationRequested();

    if (!BigInteger.TryParse(numberString, out var number))
        throw new Exception("Number has wrong format.");

    if (number < 0)
        throw new Exception("Number must be greater than or equal to zero.");

    if (number == 0 || number == 1)
        return false;

    if (number == 2)
        return true;

    if (number % 2 == 0)
        return false;

    var divisor = new BigInteger(3);
    var halfNumber = number / 2;
    
    while (divisor <= halfNumber)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (number % divisor == 0)
            return false;

        divisor += 2;
    }
    
    return true;
}

/// <summary>
/// An async function computing the square root of an integer number.
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
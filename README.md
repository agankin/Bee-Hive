# Bee Hive

![NuGet Version](https://img.shields.io/nuget/v/Bee-Hive)

A dedicated Thread Pool for computations parallelization.

It is useful for running long CPU intensive computations in background without risk of the standard .NET Thread Pool starvation.

## Features

- Provides explicit strongly typed queues for requested computations.
- Queued computations are represented by Hive Tasks that can be awaited/cancelled.
- Supports synchronous and asynchronous computations.
- Supports accumulating results of completed computations into Result Bags.
- Threads can be dynamically added for extra load and automaticly stopped after some idle time.
- Has configurable lower/upper number of running threads and idle time for threads to be stopped.

## Quick start

- [Building a Hive](#building-a-hive)
- [Working with Hive Queues](#working-with-hive-queues)
    - [Requesting computations](#requesting-computations)
    - [Enumerating Hive Tasks](#enumerating-hive-tasks)
    - [Awaiting whole Hive Queue](#awaiting-whole-hive-queue)
- [Working with Hive Tasks](#working-with-hive-tasks)
    - [Accessing Hive Tasks](#accessing-hive-tasks)
    - [Hive Task properties](#hive-task-properties)
    - [Hive Task cancellation](#hive-task-cancellation)
    - [Hive Task on errors](#hive-task-on-errors)
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

### Working with Hive Queues

Hive runs computations fetching them from Hive Queues. Computations from all queues are performed within the single Hive's Thread Pool.

#### Requesting computations

The example below shows how Hive Queues are created and computations are requested:

```cs
HiveQueue<string, bool> isPrimeQueue = hive.GetQueueFor<string, bool>(IsPrimeNumber);
HiveQueue<int, double> sqrtQueue = hive.GetQueueFor<int, double>(SqrtAsync);

HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest("1007"); // The call returns an instance of Hive Task.
_ = isPrimeQueue.AddRequest("2333");
_ = isPrimeQueue.AddRequest("5623");
_ = isPrimeQueue.AddRequest("7753");

_ = sqrtQueue.AddRequest(121);
_ = sqrtQueue.AddRequest(144);
```

#### Enumerating Hive Tasks

Requested computations are represented by instances of HiveTask&lt;TRequest, TResult&gt;.
Hive Queue implements IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt; and allows enumeration of pending or currently run Hive Tasks. Once a task completes it gets removed from the owning queue.

The example of accessing Hive Tasks via enumeration:

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

#### Awaiting whole Hive Queue

A handy extension method exists allowing to await all Hive Tasks in a queue:

```cs
await isPrimeQueue.WhenAll();        // After this line all IsPrimeNumber computations are completed.
```

### Working with Hive Tasks

A Hive Task represents a computation that will be performed by the Hive.

#### Accessing Hive Tasks

Hive Task is returned from the HiveQueue&lt;TRequest, TResult&gt;.AddRequest(TRequest request) method:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(64);
```

It can also be obtained by enumeration/applying LINQ operators to HiveQueue&lt;TRequest, TResult&gt; implementing IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt;:

```cs
sqrtQueue.AddRequest(225);
HiveTask<int, double> hiveTask = sqrtQueue.First();
```

#### Hive Task properties

Hive Tasks are awaitables:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest("1000000007");
bool isPrime = await hiveTask;
```

An extension method exists for safely awaiting without exceptions thrown:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);          // Unsupported square root of negative number.
Result<int, double> result = await hiveTask.AsyncResult();

// Matching possible states of the result: having a value of successfully completed computation, an error or cancelled.
result.Match(
    onValue: value => Console.WriteLine($"Value: {value}"),
    onError: error => Console.WriteLine($"Error: {error}"),
    onCancelled: () => Console.WriteLine($"Cancelled")
);
```

Canonical Task&lt;TResult&gt; can be obtained via Task property or by implicit conversion:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(256);

Task<double> task = hiveTask.Task;
Task<double> theSameTask = hiveTask;
```

Hive Task has properties containing initial computation request, current state and computed result:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.AddRequest("1000000009");

await hiveTask;

Debug.Assert(hiveTask.Request == "1000000009");                      // Request the computation was invoked for.
Debug.Assert(hiveTask.State == HiveTaskState.SuccessfullyCompleted); // After awaiting state becomes SuccessfullyCompleted.
Debug.Assert(hiveTask.Result?.Value == true);                        // After completion result contains computed value.
```

#### Hive Task cancellation

If cooperative cancellation is supported a Hive Task can be cancelled by calling the HiveTask&lt;TRequest, TResult&gt;.Cancel() method:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(64);
hiveTask.Cancel();

try
{
    await hiveTask;                                                   // Await for cancellation.
}
catch (TaskCanceledException) {}
Debug.Assert(hiveTask.State == HiveTaskState.Cancelled);              // Now the Task is in Cancelled state.
```

But if cancellation isn't supported this call will have no effect.

#### Hive Task on errors

A Hive Task goes to an error state if an exception occures:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.AddRequest(-16);     // Unsupported square root of negative number.

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

### Result Bags

Result Bags allow to accumulate results of completed computations. A Result Bag is created for a queue and receives results of completed computations from that queue.

Result Bags support enumeration of results and has methods for taking out elements:

- TryTake - tries to get a result without waiting and returns a flag meaning if a result is found.
- TryTakeOrWait - tries to get a result and returns immediately if a result exists or waits for some time/infinitely for a new result. Returns a flag meaning if a result is found.

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

### Hive disposal

A Hive must be disposed when it is no longer needed. On disposal all running computations receive cancellation. Idle threads are stopped immediately but busy threads continue running until their computations cancel/complete.

```cs
var hive1 = new HiveBuilder().Build();
hive1.Dispose();                           // Returns without blocking. The Hive's busy threads finish at some moment in future.

var hive2 = new HiveBuilder().Build();
await hive2.DisposeAsync();                // Awaits all threads to finish.
```

### Functions used in examples

Definition of the functions used in the examples:

```cs
/// <summary>
/// A sync function determining if an arbitrarily large number provided in the string is prime.
/// The implementation is inefficient but good as an example of long running function.
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
# Bee Hive

![NuGet Version](https://img.shields.io/nuget/v/Bee-Hive)

A dedicated Thread Pool for computations parallelization.

It is useful for running long CPU intensive computations without risk of standard .NET Thread Pool starvation.

## Features

- Computations are scheduled for execution in explicit strongly typed queues.
- Queued computations are represented as Hive tasks that can be awaited/cooperatively cancelled.
- Supports both synchronous and asynchronous computations.
- Supports any number of queues with own result bags allowing to group related computations into batches.
- Threads are dynamically added when having extra pending computations or stopped after some idle time.
- Lower/upper number of threads and max idle time before stop are explicitly set in Hive configuration.

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
    .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of an idle time for threads to be stopped.
    .Build();                                             // Builds a Hive.

hive.Run();                                               // Runs the Hive. This starts minimal number of threads.
```

### Hive Queues

Hive runs computations fetching them from Hive Queues. Computations from all Queues are executed within the single Hive's thread pool.

#### Enqueueing computations

The example below shows how Hive Queues are created and computations are added:

```cs
HiveQueue<string, bool> isPrimeQueue = hive.GetQueueFor<string, bool>(IsPrimeNumber);
HiveQueue<int, int> sqrtQueue = hive.GetQueueFor<int, double>(SqrtAsync);

HiveTask<string, bool> isPrimeHiveTask = isPrimeQueue.EnqueueCompute("1007"); // The call returns Hive Task for the computation.
_ = isPrimeQueue.EnqueueCompute("2333");
_ = isPrimeQueue.EnqueueCompute("5623");
_ = isPrimeQueue.EnqueueCompute("7753");

_ = sqrtQueue.EnqueueCompute(121);
_ = sqrtQueue.EnqueueCompute(144);
```

#### Enumerating computations

Enqueued computations are instances of HiveTask&lt;TRequest, TResult&gt;. Hive Queue implements IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt; to allow enumeration of Hive Tasks. Hive Queues contain only pending or currently executed Hive Tasks. Once a Hive Task completes it gets removed from the Hive Queue.

The example of Hive Tasks enumeration:

```cs
foreach (HiveTask<string, bool> hiveTask in isPrimeQueue)
    Console.WriteLine($"Computing is {hiveTask.Request} prime or not. State={hiveTask.State}.");

Console.WriteLine();

foreach (HiveTask<int, double> hiveTask in sqrtQueue)
    Console.WriteLine($"Computing square root of {hiveTask.Request}. State={hiveTask.State}.");
```

It prints in the terminal something like:

```
Computing is 1007 prime or not. State=InProgress.
Computing is 2333 prime or not. State=InProgress.
Computing is 5623 prime or not. State=InProgress.
Computing is 7753 prime or not. State=InProgress.

Computing square root of 121. State=Pending.
Computing square root of 144. State=Pending.
```

#### Awaiting Hive Queue

A handy extension method exists allowing to await all computations in the Hive Queue:

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

Or by enumeration or applying LINQ operators to HiveQueue&lt;TRequest, TResult&gt; implementing IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt;:

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

Canonical Task&lt;TResult&gt; can be obtained мia HiveTask&lt;TRequest, TResult&gt;.Task property or by implicit conversion:

```cs
HiveTask<int, double> hiveTask = sqrtQueue.EnqueueCompute(256);

Task<double> task = hiveTask.Task;
Task<double> theSameTask = hiveTask;
```

Hive Task has properties containing initial computation request and current state:

```cs
HiveTask<string, bool> hiveTask = isPrimeQueue.EnqueueCompute("1000000009");

await hiveTask;

Debug.Assert(hiveTask.Request == "1000000009");
Debug.Assert(hiveTask.State == HiveTaskState.SuccessfullyCompleted); // After await state is SuccessfullyCompleted.
```

#### Hive Tasks cancellation

If computation supports cooperative cancellation it can be cancelled with HiveTask&lt;TRequest, TResult&gt;.Cancel() method:

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

If cooperative cancellation is not supported the call will have no effect.

#### Hive Tasks errors

Hive Task can go to an error state if an error occures during computation:

```cs
HiveTask<int, double> hiveTask = computeSqrtQueue.EnqueueCompute(-16);        // Unsupported square root of negative number.

try
{
    await hiveTask;                                                           // Await for fail.
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);                                            // Prints "Cannot calculate sqrt of negative value.".
}
Debug.Assert(hiveTask.State == HiveTaskState.Error);                 // Now the task is in Error state.
```

### Result Bags

Often it is useful to accumulate results of computations e.g. when creating a queue and enqueueing a batch of computations and handling ready results later.

Result bags can be created for queues to receive completed results. Each result bag receives results completed after time of it's creation.

When a result bag is no longer needed it must be disposed to prevent further filling with new computed results.

Result bag has methods for taking out elements.

**IHiveResultBag&lt;TRequest, TResult&gt;.TryTake(out Result&lt;TRequest, TResult&gt; result)** tries

```cs
HiveQueue<int, int> computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);
using IHiveResultBag<int, int> resultBag = computeSqrtQueue.CreateResultBag();

// Enqueue some computations.
_ = computeSqrtQueue.EnqueueCompute(121);
_ = computeSqrtQueue.EnqueueCompute(144);
_ = computeSqrtQueue.EnqueueCompute(256);

await computeSqrtQueue.WhenAll();                          // Awaits for all computations to complete.

// Taking and displaying all items from the result bag.
while (resultBag.TryTake(out var result))
{
    Console.WriteLine($"Sqrt of {result.Request}: State={result.State}, Value={result.Value}, Error={result.Error?.Message}");
}
```

It prints in the terminal: 

```cs
// ***************** CONSOLE *******************
// Sqrt of 144: State=Success, Value=12, Error=
// Sqrt of 256: State=Success, Value=16, Error=
// Sqrt of 121: State=Success, Value=11, Error=
// *********************************************
```

```cs
// Enqueue some additional computations.
_ = computeSqrtQueue.EnqueueCompute(289);
_ = computeSqrtQueue.EnqueueCompute(-625);

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

After Hive is no longer needed it must be disposed. On Hive disposal all running computations receive cancellation. Idle threads are stopped immediately but busy threads continue running until their computations cancel/complete.

```cs
var hive1 = new HiveBuilder().Build();
hive1.Dispose();                           // Returns without blocking. Hive busy threads finish at some moment in future.

var hive2 = new HiveBuilder().Build();
await hive2.DisposeAsync();                // Awaits all threads to finish.
```

### Functions used in examples

Definition of functions used in the examples:

```cs
/// <summary>
/// A sync function determining if a number is prime.
/// Inefficient but good as example of long running function.
/// </summary>
public static bool IsPrimeNumber(string numberString, CancellationToken cancellationToken)
{
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
/// An async function computing square root of integer number.
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
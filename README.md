# Bee Hive

This is a library for creating a dedicated thread pool for parallel computations.

It is useful for running long computations performing CPU intensive background tasks without a risk of thread pool starvation.

## Features

- Computations are scheduled for run in explicit queues.
- Queued computations are represented as Hive tasks that can be awaited/cancelled.
- Supports synchronous and asynchronous computations.
- Supports any number of queues to have different batches of computations.
- Threads are dynamically added when having extra pending computations or removed after some idle time.
- Lower/upper number of threads and max idle time before removal can be set in Hive configuration.

## Quick start

- [Building and running a Hive](#building-and-running-a-hive)
- [Example functions](#example-functions)
- [Hive queues](#hive-queues)
- [Hive tasks](#hive-tasks)
- [Queue result bags](#queue-result-bags)
- [Hive disposal](#hive-disposal)

### Building and running a Hive

HiveBuilder is used to configure and build a Hive. After it's built it must be run to handle computational requests.

```cs
// Сonfigures and creates Hive using HiveBuilder.
Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)                                // Sets minimal number of threads in the Hive.
    .WithMaxLiveThreads(4)                                // Sets maximal number of threads in the Hive.
    .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of an idle time for threads to be stopped.
    .Build();                                             // Builds a hive.

// Run Hive. This starts minimal number of threads.
hive.Run();
```

### Example functions

```cs
// A sync function computing twice of integers.
// The second param "cancellationToken" can be ommited if cancellation isn't supported.
int Twice(int value, CancellationToken cancellationToken)
{
    // Simulates long work for 1 second.
    for (var i = 0; i < 10; i++)
    {
        Thread.Sleep(100);
        cancellationToken.ThrowIfCancellationRequested();
    }
    
    return value * 2;
}

// An async function computing integer square root.
// The second param "cancellationToken" can be ommited if cancellation isn't supported.
async Task<int> SqrtAsync(int value, CancellationToken cancellationToken)
{
    // Simulates long work for 1 second.
    for (var i = 0; i < 10; i++)
    {
        await Task.Delay(100);
        cancellationToken.ThrowIfCancellationRequested();
    }

    if (value < 0)
        throw new Exception("Cannot calculate sqrt of negative value.");

    var sqrtDouble = Math.Sqrt(value);
    var sqrtInt = (int)Math.Round(sqrtDouble);
    
    return sqrtInt;
}
```

### Hive queues

A Hive handles computations from computational queues. Multiple queues HiveTask&lt;TRequest, TResult&gt; can be created for a single Hive.
All queued computations are fetched and run within the same Hive's internal thread pool.

```cs
// Obtaining 2 separate queues for Twice function.
HiveQueue<int, int> computeTwiceQueue = hive.GetQueueFor<int, int>(Twice);
HiveQueue<int, int> computeTwiceQueue2 = hive.GetQueueFor<int, int>(Twice);

// Enqueueing Twice computations to the first and the second queues to be run in parallel.
_ = computeTwiceQueue.EnqueueCompute(1);
_ = computeTwiceQueue.EnqueueCompute(3);
_ = computeTwiceQueue.EnqueueCompute(5);

_ = computeTwiceQueue2.EnqueueCompute(7);
_ = computeTwiceQueue2.EnqueueCompute(9);

// Queues can be enumerated. The loop below prints 3 lines.
foreach (HiveTask<int, int> hiveTask in computeTwiceQueue)
    Console.WriteLine($"Computing twice of {hiveTask.Request}: State={hiveTask.State}.");

// ***************** CONSOLE *******************
// Computing twice of 1: State=InProgress.
// Computing twice of 3: State=InProgress.
// Computing twice of 5: State=InProgress.
// *********************************************

// The second queue has only 2 elements.
foreach (HiveTask<int, int> hiveTask in computeTwiceQueue2)
    Console.WriteLine($"Computing twice of {hiveTask.Request}: State={hiveTask.State}.");

// ***************** CONSOLE *******************
// Computing twice of 7: State=InProgress.
// Computing twice of 9: State=Pending.
// *********************************************

// Obtaining the third queue for SqrtAsync computation.
HiveQueue<int, int> computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);

// Enqueueing SqrtAsync computations to the third queue.
_ = computeSqrtQueue.EnqueueCompute(121);
_ = computeSqrtQueue.EnqueueCompute(144);

// The third queue has also 2 elements.
foreach (HiveTask<int, int> hiveTask in computeSqrtQueue)
    Console.WriteLine($"Computing sqrt of {hiveTask.Request}: State={hiveTask.State}.");

// ***************** CONSOLE *******************
// Computing sqrt of 121: State=Pending.
// Computing sqrt of 144: State=Pending.
// *********************************************
```

### Hive tasks

Computations are queued as instances of HiveTask&lt;TRequest, TResult&gt;. HiveQueue&lt;TRequest, TResult&gt; implements IReadOnlyCollection&lt;HiveTask&lt;TRequest, TResult&gt;&gt; interface allowing enumeration of pending and currently performed Hive tasks.

Each call of instance method HiveQueue&lt;TRequest, TResult&gt;.EnqueueCompute(TRequest request) also returns HiveTask&lt;TRequest, TResult&gt; representing the queued computation.

```cs
// Getting a new computation queue for SqrtAsync function.
HiveQueue<int, int> computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);
_ = computeSqrtQueue.EnqueueCompute(225);

// While computation in Pending or InProgress state it is in queue.
HiveTask<int, int> sqrtOf225HiveTask = computeSqrtQueue.Single();
var itIs15 = await sqrtOf225HiveTask;                                         // HiveTask<int, int> can be awaited.
Debug.Assert(itIs15 == 15);

// Instances of HiveTask is returned from EnqueueCompute calls.
HiveTask<int, int> sqrtOf256HiveTask = computeSqrtQueue.EnqueueCompute(256);

// Canonical Task<int> can be obtained from HiveTask via implicit conversion or Task property.
Task<int> sqrtOf256Task = sqrtOf256HiveTask;
var itIs16 = await sqrtOf256Task;
Debug.Assert(itIs16 == 16);

// After completion the task will be in SuccessfullyCompleted state.
Debug.Assert(sqrtOf256HiveTask.State == HiveTaskState.SuccessfullyCompleted);

// Tasks can be cancelled.
// Cancel will have effect only if the computation function supports cooperative cancellation.
HiveTask<int, int> hiveTaskToCancel = computeSqrtQueue.EnqueueCompute(64);
hiveTaskToCancel.Cancel();
try
{
    await hiveTaskToCancel;                                                   // Await for cancellation.
}
catch (TaskCanceledException) {}
Debug.Assert(hiveTaskToCancel.State == HiveTaskState.Cancelled);              // After completion the task is in Cancelled state.

// A task can finish with an error.
// This task is trying to perform unsupported square root calculation from a negative number.
HiveTask<int, int> hiveTaskWithError = computeSqrtQueue.EnqueueCompute(-16);
try
{
    await hiveTaskWithError;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);                                            // Prints "Cannot calculate sqrt of negative value.".
}
Debug.Assert(hiveTaskWithError.State == HiveTaskState.Error);                 // After completion the task is in Error state.
```

### Queue result bags

Results of queued computations can be accumulated in result bags. Each result bag receives new results computed after time of it's creation.

When a result bag is no longer needed it must be disposed to prevent filling with new computed results.

```cs
HiveQueue<int, int> computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);
using IHiveResultBag<int, int> resultBag = computeSqrtQueue.CreateResultBag();

// Enqueue some computations.
_ = computeSqrtQueue.EnqueueCompute(121);
_ = computeSqrtQueue.EnqueueCompute(144);
_ = computeSqrtQueue.EnqueueCompute(256);

// Await for all computations to complete.
await computeSqrtQueue.WhenAll();

// Taking and displaying all items from the result bag.
while (resultBag.TryTake(out var result))
{
    Console.WriteLine($"Sqrt of {result.Request}: State={result.State}, Value={result.Value}, Error={result.Error?.Message}");
}

// ***************** CONSOLE *******************
// Sqrt of 144: State=Success, Value=12, Error=
// Sqrt of 256: State=Success, Value=16, Error=
// Sqrt of 121: State=Success, Value=11, Error=
// *********************************************

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

After Hive is no longer needed it must be disposed. On disposal running computations receive cancellation.
Idle threads are stopped immediately but busy threads continue running untill their current computations cancel/complete.

```cs
var hive1 = new HiveBuilder().Build();
hive1.Dispose();                           // Returns without blocking. Hive busy threads finish at some moment in future.

var hive2 = new HiveBuilder().Build();
await hive2.DisposeAsync();                // Awaits all threads to finish.
```
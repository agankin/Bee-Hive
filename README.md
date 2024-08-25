# Bee Hive

This is a library for creating a dedicated thread pool for parallel computations.

It is useful for running long computations performing CPU intensive background tasks without a risk of thread pool starvation.

## Features

- Computations are scheduled for run in explicit queues.
- Supports synchronous and asynchronous computations.
- Queued computations are presented as tasks that can be awaited.
- Pending/progressing tasks in queues can be enumerated and cancelled.
- Threads are dynamically added when having extra pending computations or removed after some idle time.
- Lower/upper number of threads and max idle time before removal can be set in Hive configuration.

## Usage Samples

### Configuring and creating a Hive

```cs
// Сonfigure and create Hive using HiveBuilder.
Hive hive = new HiveBuilder()
    .WithMinLiveThreads(1)                                // Sets minimal number of threads in the Hive.
    .WithMaxLiveThreads(4)                                // Sets maximal number of threads in the Hive.
    .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of an idle time for threads to be stopped.
    .Build();                                             // Creates a hive.

// Run Hive. This starts minimal number of threads.
hive.Run();
```

### Functions used in examples

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

### Obtaining queues and enqueuing computations

```cs
// Obtaining 2 separate queues for Twice function.
var computeTwiceQueue = hive.GetQueueFor<int, int>(Twice);
var computeTwiceQueue2 = hive.GetQueueFor<int, int>(Twice);

// Enqueueing Twice computations to be run in parallel to the first queue.
computeTwiceQueue.EnqueueCompute(1);
computeTwiceQueue.EnqueueCompute(3);
computeTwiceQueue.EnqueueCompute(5);

// Enqueueing Twice computations to the second queue.
computeTwiceQueue2.EnqueueCompute(7);
computeTwiceQueue2.EnqueueCompute(9);

// The queue can be enumerated. The loop below prints 3 lines.
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

// Obtaining a third queue for SqrtAsync computation.
var computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);

// Enqueueing SqrtAsync computations to the third queue.
computeSqrtQueue.EnqueueCompute(121);
computeSqrtQueue.EnqueueCompute(144);

// The third queue has also 2 elements.
foreach (HiveTask<int, int> hiveTask in computeSqrtQueue)
    Console.WriteLine($"Computing sqrt of {hiveTask.Request}: State={hiveTask.State}.");

// ***************** CONSOLE *******************
// Computing sqrt of 121: State=Pending.
// Computing sqrt of 144: State=Pending.
// *********************************************
```

### Working with queued tasks

```cs
// Getting a new computation queue for SqrtAsync function:
var computeSqrtQueue = hive.GetQueueFor<int, int>(SqrtAsync);
computeSqrtQueue.EnqueueCompute(225);

// While computation in Pending or InProgress state it is in queue:
HiveTask<int, int> sqrtOf225HiveTask = computeSqrtQueue.Single();

// HiveTask<int, int> can be awaited.
var itIs15 = await sqrtOf225HiveTask;

// An instance of HiveTask can be received from the EnqueueCompute call:
HiveTask<int, int> sqrtOf256HiveTask = computeSqrtQueue.EnqueueCompute(256);

// Canonical Task<int> can be obtained from HiveTask via implicit conversion or Task property.
Task<int> sqrtOf256Task = sqrtOf256HiveTask;
var itIs16 = await sqrtOf256Task;

// After complete the task will be in SuccessfullyCompleted state.
var successfullyCompletedState = sqrtOf256HiveTask.State;

// Tasks can be cancelled.
// Cancel will have effect only if the computation function supports cooperative cancellation.
HiveTask<int, int> hiveTaskToCancel = computeSqrtQueue.EnqueueCompute(64);
hiveTaskToCancel.Cancel();

// Await for cancellation.
await hiveTaskToCancel;

// After complete the task will be in Cancelled state.
var cancelledState = hiveTaskToCancel.State; // cancelledState is HiveTaskState.Cancelled.

// A task can finish with an error.
// This task is trying to perform unsupported square root calculation from a negative number.
var hiveTaskWithError = computeSqrtQueue.EnqueueCompute(-16);

try
{
    await hiveTaskWithError;
}
catch (Exception ex)
{
    // This prints "Cannot calculate sqrt of negative value.".
    Console.WriteLine(ex.Message);
}

var errorState = hiveTaskWithError.State; // errorState is HiveTaskState.Error.
```
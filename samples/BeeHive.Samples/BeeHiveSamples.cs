namespace BeeHive.Samples;

using System.Diagnostics;
using static BeeHive.Samples.ComputationFunctions;

internal static class BeeHiveSamples
{
    public static void HiveQueuesSample()
    {
        using var hive = RunHive();

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
    }

    public static async Task HiveTasksSample()
    {
        using var hive = RunHive();

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
    }

    public static async Task QueueResultBagSample()
    {
        using var hive = RunHive();
        
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
    }

    private static Hive RunHive()
    {
        // Сonfigures and creates Hive using HiveBuilder.
        Hive hive = new HiveBuilder()
            .WithMinLiveThreads(1)                                // Sets minimal number of threads in the Hive.
            .WithMaxLiveThreads(4)                                // Sets maximal number of threads in the Hive.
            .WithThreadIdleBeforeStop(milliseconds: 1000)         // Sets interval of an idle time for threads to be stopped.
            .Build();                                             // Builds a hive.

        // Run Hive. This starts minimal number of threads.
        hive.Run();

        return  hive;
    }
}
using BenchmarkDotNet.Attributes;

namespace BeeHive.Benchmarks;

using static ComputationFunctions;

public class Benchmarks
{
    private const int Sum_Up_To_Count = 10_000;
    private const long Expected_Tenfold_Total_After_Sum_Up = 500_050_000;
    private const int DegreeOfParallelism = 4;

    private Hive _hive = null!;

    [GlobalSetup]
    public void Setup()
    {
        _hive = new HiveBuilder()
            .WithMinLiveThreads(DegreeOfParallelism)
            .WithMaxLiveThreads(DegreeOfParallelism)
            .Build()
            .Run();

        ThreadPool.SetMinThreads(DegreeOfParallelism, DegreeOfParallelism);
        ThreadPool.SetMaxThreads(DegreeOfParallelism, DegreeOfParallelism);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _hive.Dispose();
    }

    [Benchmark(Description = "Run sync tasks in Thread Pool")]
    public void RunThreadPoolSyncTasks()
    {
        var tasks = Enumerable.Range(0, Sum_Up_To_Count + 1)
            .Select(value => Task.Run(() => TenfoldSync(value)))
            .ToArray();
        Task.WaitAll(tasks);
        
        AssertExpectedTenfoldSum(tasks);
    }

    [Benchmark(Description = "Run sync tasks in Hive")]
    public void RunHiveSyncTasks()
    {
        var queue = _hive.GetQueueFor<long, long>(TenfoldSync);

        var tasks = Enumerable.Range(0, Sum_Up_To_Count + 1)
            .Select(value => queue.EnqueueCompute(value).Task)
            .ToArray();
        Task.WaitAll(tasks);
        
        AssertExpectedTenfoldSum(tasks);
    }

    [Benchmark(Description = "Run async tasks in Thread Pool")]
    public void RunThreadPoolAsyncTasks()
    {
        var tasks = Enumerable.Range(0, Sum_Up_To_Count + 1)
            .Select(value => Task.Run(async () => await TenfoldAsync(value)))
            .ToArray();
        Task.WaitAll(tasks);
        
        AssertExpectedTenfoldSum(tasks);
    }

    [Benchmark(Description = "Run async tasks in Hive")]
    public void RunHiveAsyncTasks()
    {
        var queue = _hive.GetQueueFor<long, long>(TenfoldAsync);

        var tasks = Enumerable.Range(0, Sum_Up_To_Count + 1)
            .Select(value => queue.EnqueueCompute(value).Task)
            .ToArray();
        Task.WaitAll(tasks);
        
        AssertExpectedTenfoldSum(tasks);
    }

    private static void AssertExpectedTenfoldSum(IEnumerable<Task<long>> tasks)
    {
        var tenfoldSum = tasks.Select(task => task.Result).Sum();

        if (tenfoldSum != Expected_Tenfold_Total_After_Sum_Up)
            throw new Exception($"Expected tenfold sum is {Expected_Tenfold_Total_After_Sum_Up} but computed value is {tenfoldSum}.");
    }
}
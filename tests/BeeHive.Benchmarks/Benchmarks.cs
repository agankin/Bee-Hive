﻿using BenchmarkDotNet.Attributes;

namespace BeeHive.Benchmarks;

public class Benchmarks
{
    private const int ComputeRunCount = 100;
    private const int ComputeSumOfNumbers = 1000000;
    private const int DegreeOfParallelism = 4;

    private Hive<Unit, int> _hive = null!;

    [GlobalSetup]
    public void Setup()
    {
        _hive = Hive.ToCompute<Unit, int>(Compute)
            .WithMinLiveThreads(DegreeOfParallelism)
            .WithMaxLiveThreads(DegreeOfParallelism)
            .Build();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _hive.Dispose();
    }

    [Benchmark]
    public void SequentialComputation()
    {
        for (var runIdx = 0; runIdx < ComputeRunCount; runIdx++)
        {
            var _ = Compute(Unit.Value);
        }
    }

    [Benchmark]
    public void PLinqComputation()
    {
        var results = Enumerable.Range(0, ComputeRunCount)
            .AsParallel().WithDegreeOfParallelism(DegreeOfParallelism)
            .Select(_ => Compute(Unit.Value));
        foreach (var result in results)
        {
        }
    }

    [Benchmark]
    public void HiveComputation()
    {
        var results = _hive.CreateResultCollection().GetConsumingEnumerable();

        for (var runIdx = 0; runIdx < ComputeRunCount; runIdx++)
        {
            _hive.Compute(Unit.Value);
        }

        var resultCount = 0;
        foreach (var result in results)
        {
            if (++resultCount == ComputeRunCount)
            {
                return;
            }
        }
    }

    [Benchmark]
    public void ThreadedComputation()
    {
        var threads = new List<Thread>();

        Thread CreateComputationThread(int computeRunCount) => new(() => {
                for (var runIdx = 0; runIdx < computeRunCount; runIdx++)
                {
                    var result = Compute(Unit.Value);
                }
            });

        var computeRunCountPerThread = (int)Math.Ceiling(1.0 * ComputeRunCount / DegreeOfParallelism);
        for (var parallelIdx = 0; parallelIdx < DegreeOfParallelism - 1; parallelIdx++)
        {
            var thread = CreateComputationThread(computeRunCountPerThread);
            thread.Start();
            
            threads.Add(thread);
        }

        var runCountLeft = ComputeRunCount - computeRunCountPerThread * (DegreeOfParallelism - 1);
        var lastThread = CreateComputationThread(runCountLeft);
        lastThread.Start();

        threads.Add(lastThread);

        foreach (var thread in threads)
            thread.Join();
    }

    private static int Compute(Unit unit)
    {
        var result = 0;
        for (var number = 1; number <= ComputeSumOfNumbers; number++)
        {
            result += number;
        }

        return result;
    }

    private readonly struct Unit
    {
        public static readonly Unit Value = new();
    }
}
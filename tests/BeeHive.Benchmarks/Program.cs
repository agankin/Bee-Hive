using BeeHive.Benchmarks;
using BenchmarkDotNet.Running;

var _ = BenchmarkRunner.Run<Benchmarks>();

Console.ReadKey(true);
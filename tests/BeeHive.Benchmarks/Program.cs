﻿using BeeHive.Benchmarks;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<Benchmarks>();

Console.ReadKey(true);
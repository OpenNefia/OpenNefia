﻿using BenchmarkDotNet.Attributes;
using OpenNefia.Core.Serialization.Manager;

namespace OpenNefia.Benchmarks.Serialization.Initialize
{
    [MemoryDiagnoser]
    public class SerializationInitializeBenchmark : SerializationBenchmark
    {
        [IterationCleanup]
        public void IterationCleanup()
        {
            SerializationManager.Shutdown();
        }

        [Benchmark]
        public ISerializationManager Initialize()
        {
            InitializeSerialization();
            return SerializationManager;
        }
    }
}

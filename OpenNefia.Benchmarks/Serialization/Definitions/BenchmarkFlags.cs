using System;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Benchmarks.Serialization.Definitions
{
    public class BenchmarkFlags
    {
        public const int Zero = 1 << 0;
        public const int ThirtyOne = 1 << 31;
    }

    [Flags]
    [FlagsFor(typeof(BenchmarkFlags))]
    public enum BenchmarkFlagsEnum
    {
        Zero = BenchmarkFlags.Zero,
        ThirtyOne = BenchmarkFlags.ThirtyOne
    }
}

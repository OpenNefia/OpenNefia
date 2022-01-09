using System.Collections.Generic;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Random
{
    public interface IRandom
    {
        float NextFloat();
        public float NextFloat(float minValue, float maxValue)
            => NextFloat() * (maxValue - minValue) + minValue;
        public float NextFloat(float maxValue) => NextFloat() * maxValue;
        int Next();
        int Next(int minValue, int maxValue);
        int Next(int maxValue);
        double NextDouble();
        void NextBytes(byte[] buffer);

        void PushSeed(int seed);
        void PopSeed();
        void ClearPushedSeeds();
    }
}

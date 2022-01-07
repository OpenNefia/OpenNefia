using System;
using System.Collections.Generic;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Random
{
    public static class RandomExtensions
    {
        /// <summary>
        ///     Generate a random number from a normal (gaussian) distribution.
        /// </summary>
        /// <param name="random">The random object to generate the number from.</param>
        /// <param name="μ">The average or "center" of the normal distribution.</param>
        /// <param name="σ">The standard deviation of the normal distribution.</param>
        public static double NextGaussian(this IRandom random, double μ = 0, double σ = 1)
        {
            // https://stackoverflow.com/a/218600
            var α = random.NextDouble();
            var β = random.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(α)) * Math.Sin(2.0 * Math.PI * β);

            return μ + σ * randStdNormal;
        }

        public static T Pick<T>(this IRandom random, IReadOnlyList<T> list)
        {
            var index = random.Next(list.Count);
            return list[index];
        }

        public static T? PickOrDefault<T>(this IRandom random, IReadOnlyList<T> list)
        {
            if (list.Count == 0)
                return default;

            var index = random.Next(list.Count);
            return list[index];
        }

        /// <summary>Picks a random element from a collection.</summary>
        /// <remarks>
        ///     This is O(n).
        /// </remarks>
        public static T Pick<T>(this IRandom random, IReadOnlyCollection<T> collection)
        {
            var index = random.Next(collection.Count);
            var i = 0;
            foreach (var t in collection)
            {
                if (i++ == index)
                {
                    return t;
                }
            }

            throw new InvalidOperationException("This should be unreachable!");
        }

        /// <summary>
        /// Picks a random element from a list and removes it.
        /// </summary>
        public static T PickAndTake<T>(this IRandom random, IList<T> list)
        {
            var index = random.Next(list.Count);
            var element = list[index];
            list.RemoveAt(index);
            return element;
        }

        public static float NextFloat(this IRandom random)
        {
            // This is pretty much the CoreFX implementation.
            // So credits to that.
            // Except using float instead of double.
            return random.Next() * 4.6566128752458E-10f;
        }

        public static float NextFloat(this System.Random random)
        {
            return random.Next() * 4.6566128752458E-10f;
        }

        public static Vector2i NextPoint(this IRandom random, UIBox2i bounds)
        {
            return new Vector2i(random.Next(bounds.Left, bounds.Right),
                                random.Next(bounds.Top, bounds.Bottom));
        }

        /// <summary>
        ///     Have a certain chance to return a boolean.
        /// </summary>
        /// <param name="random">The random instance to run on.</param>
        /// <param name="chance">The chance to pass, from 0 to 1.</param>
        public static bool Prob(this IRandom random, float chance)
        {
            DebugTools.Assert(chance <= 1 && chance >= 0, $"Chance must be in the range 0-1. It was {chance}.");

            return random.NextDouble() <= chance;
        }

        /// <summary>
        /// Returns true one out of N times.
        /// </summary>
        /// <param name="random">The random instance to run on.</param>
        /// <param name="chance">The chance to pass, 1 or greater</param>
        public static bool OneIn(this IRandom random, int chance)
        {
            DebugTools.Assert(chance > 1, $"Chance must be greater than 1. It was {chance}.");

            return random.Next(chance) == 0;
        }

        /// <summary>
        /// Randomly shuffles a list.
        /// </summary>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(this IRandom random, IList<T> list)
        {
            var n = list.Count;
            while (n > 1)
            {
                n -= 1;
                var k = random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

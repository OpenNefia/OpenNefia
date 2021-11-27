using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Random
{
    /// <summary>
    /// Static wrapper around <see cref="IRandom"/>, since it's heavily used.
    /// </summary>
    public static class Rand
    {
        private static IRandom _random => IoCManager.Resolve<IRandom>();

        #region IRandom Methods

        public static float NextFloat()
        {
            return _random.NextFloat();
        }

        public static int Next()
        {
            return _random.Next();
        }

        public static int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        public static int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public static double NextDouble()
        {
            return _random.NextDouble();
        }

        public static void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }

        #endregion

        #region Extension Methods

        /// <summary>
        ///     Generate a random number from a normal (gaussian) distribution.
        /// </summary>
        /// <param name="random">The random object to generate the number from.</param>
        /// <param name="μ">The average or "center" of the normal distribution.</param>
        /// <param name="σ">The standard deviation of the normal distribution.</param>
        public static double NextGaussian(double μ = 0, double σ = 1)
        {
            return _random.NextGaussian(μ, σ);
        }

        public static T Pick<T>(IReadOnlyList<T> list)
        {
            return _random.Pick(list);
        }

        /// <summary>Picks a random element from a collection.</summary>
        /// <remarks>
        ///     This is O(n).
        /// </remarks>
        public static T Pick<T>(IReadOnlyCollection<T> collection)
        {
            return _random.Pick(collection);
        }

        /// <summary>
        /// Picks a random element from a list and removes it.
        /// </summary>
        public static T PickAndTake<T>(IList<T> list)
        {
            return _random.PickAndTake(list);
        }

        /// <summary>
        ///     Have a certain chance to return a boolean.
        /// </summary>
        /// <param name="random">The random instance to run on.</param>
        /// <param name="chance">The chance to pass, from 0 to 1.</param>
        public static bool Prob(float chance)
        {
            return _random.Prob(chance);
        }

        /// <summary>
        /// Returns true one out of N times.
        /// </summary>
        /// <param name="random">The random instance to run on.</param>
        /// <param name="chance">The chance to pass, 1 or greater</param>
        public static bool OneIn(int chance)
        {
            return _random.OneIn(chance);
        }

        /// <summary>
        /// Randomly shuffles a list.
        /// </summary>
        /// <param name="list">The list to shuffle.</param>
        public static void Shuffle<T>(IList<T> list)
        {
            _random.Shuffle(list);
        }

        #endregion
    }
}

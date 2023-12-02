using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomGen
{
    /// <summary>
    /// Picks from a set of candidates with a weight attached to them.
    /// A higher weight increases the chance the candidate will be picked.
    /// </summary>
    /// <typeparam name="T">Type of candidate to choose.</typeparam>
    public class WeightedSampler<T>
    {
        private record Candidate(T Value, int Sum);

        private int _sum = 0;
        private List<Candidate> _candidates = new();

        /// <summary>
        /// Clears all current candidates.
        /// </summary>
        public void Clear()
        {
            _sum = 0;
            _candidates.Clear();
        }

        /// <summary>
        /// Adds a new candidate.
        /// </summary>
        /// <param name="value">Candidate to add.</param>
        /// <param name="weight">Weight of the candidate. A higher weight increases the chance the candidate will be picked. Minimum of weight 1 will be applied.</param>
        public void Add(T value, int weight)
        {
            _sum += Math.Max(weight, 1);
            _candidates.Add(new(value, _sum));
        }

        public int Count => _candidates.Count;

        /// <summary>
        /// Samples from the current collection; returns <c>null</c> if there are no candidates.
        /// </summary>
        public T? Sample(IRandom? random = null)
        {
            IoCManager.Resolve(ref random);

            if (_sum == 0 || _candidates.Count == 0)
                return default(T);

            var n = random.Next(_sum);

            foreach (var candidate in _candidates)
            {
                if (candidate.Sum > n)
                    return candidate.Value;
            }

            return default(T);
        }

        /// <summary>
        /// Samples from the current collection; throws an error if there are no candidates.
        /// </summary>
        public T EnsureSample(IRandom? random = null)
        {
            IoCManager.Resolve(ref random);

            if (_sum == 0 || _candidates.Count == 0)
                throw new InvalidOperationException("Invalid candidates list");

            var n = random.Next(_sum);

            foreach (var candidate in _candidates)
            {
                if (candidate.Sum > n)
                    return candidate.Value;
            }

            throw new InvalidOperationException("Invalid candidates list"); ;
        }
    }
}

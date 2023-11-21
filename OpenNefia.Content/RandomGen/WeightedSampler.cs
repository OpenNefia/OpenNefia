using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomGen
{
    public class WeightedSampler<T>
    {
        private record Candidate(T Value, int Sum);

        private int _sum = 0;
        private List<Candidate> _candidates = new();

        public void Clear()
        {
            _sum = 0;
            _candidates.Clear();
        }

        public void Add(T value, int weight)
        {
            _sum += weight;
            _candidates.Add(new(value, _sum));
        }

        public int Count => _candidates.Count;

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

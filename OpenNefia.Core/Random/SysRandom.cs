using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Random
{
    public class SysRandom : IRandom
    {
        private System.Random _random = new();
        private Stack<System.Random> _stack = new();

        public float NextFloat()
        {
            return _random.NextFloat();
        }

        public int Next()
        {
            return _random.Next();
        }

        public int Next(int minValue, int maxValue)
        {
            return _random.Next(int.Max(minValue, 0), int.Max(maxValue, 0));
        }

        public int Next(int maxValue)
        {
            return _random.Next(int.Max(maxValue, 0));
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }

        public void RandomizeSeed()
        {
            _random = new System.Random();
        }

        [Obsolete("Replace public interface with WithSeed()")]
        public void PushSeed(int seed)
        {
            _stack.Push(_random);
            _random = new System.Random(seed);
        }

        [Obsolete("Replace public interface with WithSeed()")]
        public void PopSeed()
        {
            if (_stack.Count > 0)
                _random = _stack.Pop();
            else
                RandomizeSeed();
        }
    }
}

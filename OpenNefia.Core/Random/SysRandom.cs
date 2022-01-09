namespace OpenNefia.Core.Random
{
    public class SysRandom : IRandom
    {
        private System.Random _random = new();

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
            return _random.Next(minValue, maxValue);
        }

        public int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        public double NextDouble()
        {
            return _random.NextDouble();
        }

        public void NextBytes(byte[] buffer)
        {
            _random.NextBytes(buffer);
        }

        public void PushSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        public void PopSeed()
        {
            throw new NotImplementedException();
        }

        public void ClearPushedSeeds()
        {
            throw new NotImplementedException();
        }
    }
}

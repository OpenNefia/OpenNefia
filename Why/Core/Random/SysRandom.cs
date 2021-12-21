namespace Why.Core.Random
{
    public class SysRandom : IRandom
    {
        private readonly System.Random _random = new();

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
    }
}

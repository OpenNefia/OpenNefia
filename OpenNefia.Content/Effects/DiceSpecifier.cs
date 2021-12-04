using OpenNefia.Core.Random;

namespace OpenNefia.Content.Effects
{
    public interface IDice
    {
        public int Roll(IRandom random);
    }

    public class DiceSpecifier : IDice
    {
        public int DiceX = 1;
        public int DiceY = 1;
        public int Bonus = 0;

        public int Roll(IRandom random)
        {
            var x = Math.Max(DiceX, 1);
            var y = Math.Max(DiceY, 1);

            var result = 0;
            for (int i = 0; i < x; i++)
            {
                result += random.Next(1, y);
            }

            return result;
        }
    }
}
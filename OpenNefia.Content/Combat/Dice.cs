using System.Text;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Combat
{
    [ImplicitDataDefinitionForInheritors]
    public interface IDice
    {
        public int Roll(IRandom random);
        public int MaxRoll();
    }

    /// <summary>
    /// (X)d(Y)+(Bonus)
    /// </summary>
    [DataDefinition]
    public class Dice : IDice
    {
        public Dice() {}
        
        public Dice(int x, int y, int bonus)
        {
            X = x;
            Y = y;
            Bonus = bonus;
        }

        [DataField]
        public int X { get; set; }

        [DataField]
        public int Y { get; set; }

        [DataField]
        public int Bonus { get; set; }

        public int Roll(IRandom random)
        {
            var x = Math.Max(X, 1);
            var y = Math.Max(Y, 1);

            var result = 0;
            for (int i = 0; i < x; i++)
            {
                result += random.Next(1, y);
            }

            return result + Bonus;
        }

        public int MaxRoll()
        {
            return X * Y + Bonus;
        }

        public override string ToString()
        {
            var sb = new StringBuilder($"{X}d{Y}");
            if (Bonus != 0)
                sb.Append($"+{Bonus}");
            return sb.ToString();
        }
    }
}
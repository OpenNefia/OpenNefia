using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.UI
{
    public record RawKey : IKeybind
    {
        public static Dictionary<Keys, RawKey> AllKeys;

        static RawKey()
        {
            AllKeys = GenerateAllModifierVariants();
        }

        public Keys Key { get; }

        public RawKey(Keys key) { this.Key = key; }

        public bool IsShiftDelayed => false;

        private static bool IsVirtualKey(Keys key)
        {
            if ((Keys.AllModifiers & key) != Keys.None)
                return true;

            if (key == Keys.None)
                return true;

            return false;
        }

        private static Keys[] Modifiers = new Keys[]
        {
            Keys.None,
            Keys.Ctrl,
            Keys.Alt,
            Keys.Shift,
            Keys.GUI
        };

        private static IEnumerable<Keys> GenerateModifierVariants(Keys k)
        {
            HashSet<Keys> allCombos = new HashSet<Keys>();

            for (int i = 1; i < (1 << Modifiers.Length); i++)
            {
                Keys working = k;
                int index = 0;
                int checker = i;
                while (checker != 0)
                {
                    if ((checker & 0x01) == 0x01) working |= Modifiers[index];
                    checker = checker >> 1;
                    index++;
                }
                allCombos.Add(working);
            }

            return allCombos;
        }

        private static Dictionary<Keys, RawKey> GenerateAllModifierVariants()
        {
            Logger.InfoS("rawkey", $"Generating key modifier combinations.");

            return EnumHelpers.EnumerateValues<Keys>().Where((k) => !IsVirtualKey(k))
                .Distinct()
                .SelectMany((k) => GenerateModifierVariants(k))
                .ToDictionary((k) => k, (k) => new RawKey(k));
        }
    }
}

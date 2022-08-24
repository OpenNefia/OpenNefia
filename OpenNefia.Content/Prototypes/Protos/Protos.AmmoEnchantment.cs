using AmmoEnchantmentPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Enchantments.AmmoEnchantmentPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class AmmoEnchantment
        {
            #pragma warning disable format

            public static readonly AmmoEnchantmentPrototypeId Rapid = new($"Elona.{nameof(Rapid)}");
            public static readonly AmmoEnchantmentPrototypeId Vopal = new($"Elona.{nameof(Vopal)}");
            public static readonly AmmoEnchantmentPrototypeId Time  = new($"Elona.{nameof(Time)}");
            public static readonly AmmoEnchantmentPrototypeId Magic = new($"Elona.{nameof(Magic)}");
            public static readonly AmmoEnchantmentPrototypeId Bomb  = new($"Elona.{nameof(Bomb)}");
            public static readonly AmmoEnchantmentPrototypeId Burst = new($"Elona.{nameof(Burst)}");

            #pragma warning restore format
        }
    }
}

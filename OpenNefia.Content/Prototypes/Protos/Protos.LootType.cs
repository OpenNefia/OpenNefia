using LootTypePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Loot.LootTypePrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class LootType
        {
            #pragma warning disable format

            public static readonly LootTypePrototypeId Animal   = new($"Elona.{nameof(Animal)}");
            public static readonly LootTypePrototypeId Insect   = new($"Elona.{nameof(Insect)}");
            public static readonly LootTypePrototypeId Humanoid = new($"Elona.{nameof(Humanoid)}");
            public static readonly LootTypePrototypeId Drake    = new($"Elona.{nameof(Drake)}");
            public static readonly LootTypePrototypeId Dragon   = new($"Elona.{nameof(Dragon)}");
            public static readonly LootTypePrototypeId Lich     = new($"Elona.{nameof(Lich)}");

            #pragma warning restore format
        }
    }
}

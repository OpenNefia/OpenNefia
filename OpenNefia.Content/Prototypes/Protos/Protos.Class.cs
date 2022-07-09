using ClassPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Prototypes.ClassPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Class
        {
            #pragma warning disable format

            public static readonly ClassPrototypeId Warrior  = new($"Elona.{nameof(Warrior)}");
            public static readonly ClassPrototypeId Pianist  = new($"Elona.{nameof(Pianist)}");
            public static readonly ClassPrototypeId Tourist  = new($"Elona.{nameof(Tourist)}");
            public static readonly ClassPrototypeId Priest   = new($"Elona.{nameof(Priest)}");
            public static readonly ClassPrototypeId Claymore = new($"Elona.{nameof(Claymore)}");
            public static readonly ClassPrototypeId Predator = new($"Elona.{nameof(Predator)}");
            public static readonly ClassPrototypeId Warmage  = new($"Elona.{nameof(Warmage)}");
            public static readonly ClassPrototypeId Farmer   = new($"Elona.{nameof(Farmer)}");
            public static readonly ClassPrototypeId Archer   = new($"Elona.{nameof(Archer)}");
            public static readonly ClassPrototypeId Thief    = new($"Elona.{nameof(Thief)}");
            public static readonly ClassPrototypeId Wizard   = new($"Elona.{nameof(Wizard)}");
            public static readonly ClassPrototypeId Gunner   = new($"Elona.{nameof(Gunner)}");

            #pragma warning restore format
        }
    }
}

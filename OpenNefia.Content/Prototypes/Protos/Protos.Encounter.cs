using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Encounter
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId Enemy    = new($"Elona.Encounter{nameof(Enemy)}");
            public static readonly EntityPrototypeId Merchant = new($"Elona.Encounter{nameof(Merchant)}");
            public static readonly EntityPrototypeId Assassin = new($"Elona.Encounter{nameof(Assassin)}");
            public static readonly EntityPrototypeId Rogue    = new($"Elona.Encounter{nameof(Rogue)}");

            #pragma warning restore format
        }
    }
}

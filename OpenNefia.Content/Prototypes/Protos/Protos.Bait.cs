using BaitPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Fishing.BaitPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Bait
        {
            #pragma warning disable format

            public static readonly BaitPrototypeId WaterFlea   = new($"Elona.{nameof(WaterFlea)}");
            public static readonly BaitPrototypeId Grasshopper = new($"Elona.{nameof(Grasshopper)}");
            public static readonly BaitPrototypeId Ladybug     = new($"Elona.{nameof(Ladybug)}");
            public static readonly BaitPrototypeId Dragonfly   = new($"Elona.{nameof(Dragonfly)}");
            public static readonly BaitPrototypeId Locust      = new($"Elona.{nameof(Locust)}");
            public static readonly BaitPrototypeId Beetle      = new($"Elona.{nameof(Beetle)}");

            #pragma warning restore format
        }
    }
}

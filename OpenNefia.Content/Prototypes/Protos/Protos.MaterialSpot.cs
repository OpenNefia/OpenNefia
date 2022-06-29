using MaterialSpotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.MaterialSpot.MaterialSpotPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class MaterialSpot
        {
            #pragma warning disable format

            public static readonly MaterialSpotPrototypeId Global   = new($"Elona.{nameof(Global)}");
            public static readonly MaterialSpotPrototypeId Building = new($"Elona.{nameof(Building)}");
            public static readonly MaterialSpotPrototypeId Water    = new($"Elona.{nameof(Water)}");
            public static readonly MaterialSpotPrototypeId Mine     = new($"Elona.{nameof(Mine)}");
            public static readonly MaterialSpotPrototypeId Bush     = new($"Elona.{nameof(Bush)}");
            public static readonly MaterialSpotPrototypeId Field    = new($"Elona.{nameof(Field)}");
            public static readonly MaterialSpotPrototypeId Dungeon  = new($"Elona.{nameof(Dungeon)}");
            public static readonly MaterialSpotPrototypeId Forest   = new($"Elona.{nameof(Forest)}");
            public static readonly MaterialSpotPrototypeId General  = new($"Elona.{nameof(General)}");
            public static readonly MaterialSpotPrototypeId Remains  = new($"Elona.{nameof(Remains)}");

            #pragma warning restore format
        }
    }
}

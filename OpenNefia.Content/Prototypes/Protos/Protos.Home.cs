using HomePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Home.HomePrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Home
        {
            #pragma warning disable format

            public static readonly HomePrototypeId Cave        = new($"Elona.{nameof(Cave)}");
            public static readonly HomePrototypeId Shack       = new($"Elona.{nameof(Shack)}");
            public static readonly HomePrototypeId CozyHouse   = new($"Elona.{nameof(CozyHouse)}");
            public static readonly HomePrototypeId Estate      = new($"Elona.{nameof(Estate)}");
            public static readonly HomePrototypeId CyberHouse  = new($"Elona.{nameof(CyberHouse)}");
            public static readonly HomePrototypeId SmallCastle = new($"Elona.{nameof(SmallCastle)}");

            #pragma warning restore format
        }
    }
}

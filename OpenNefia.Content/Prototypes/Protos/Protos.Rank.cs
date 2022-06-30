using RankPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Ranks.RankPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Rank
        {
            #pragma warning disable format

            public static readonly RankPrototypeId Arena    = new($"Elona.{nameof(Arena)}");
            public static readonly RankPrototypeId PetArena = new($"Elona.{nameof(PetArena)}");
            public static readonly RankPrototypeId Crawler  = new($"Elona.{nameof(Crawler)}");
            public static readonly RankPrototypeId Museum   = new($"Elona.{nameof(Museum)}");
            public static readonly RankPrototypeId Home     = new($"Elona.{nameof(Home)}");
            public static readonly RankPrototypeId Shop     = new($"Elona.{nameof(Shop)}");
            public static readonly RankPrototypeId Vote     = new($"Elona.{nameof(Vote)}");
            public static readonly RankPrototypeId Fishing  = new($"Elona.{nameof(Fishing)}");
            public static readonly RankPrototypeId Guild    = new($"Elona.{nameof(Guild)}");

            #pragma warning restore format
        }
    }
}

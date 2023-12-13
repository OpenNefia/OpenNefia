using MapPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class Map
        {
            #pragma warning disable format

            public static readonly MapPrototypeId Nefia = new($"Elona.Map{nameof(Nefia)}");
            public static readonly MapPrototypeId QuestHunt = new($"Elona.Map{nameof(QuestHunt)}");
            public static readonly MapPrototypeId QuestHuntEX = new($"Elona.Map{nameof(QuestHuntEX)}");
            public static readonly MapPrototypeId QuestConquer = new($"Elona.Map{nameof(QuestConquer)}");
            public static readonly MapPrototypeId QuestParty = new($"Elona.Map{nameof(QuestParty)}");
            public static readonly MapPrototypeId QuestHarvest = new($"Elona.Map{nameof(QuestHarvest)}");

            #pragma warning restore format
        }
    }
}

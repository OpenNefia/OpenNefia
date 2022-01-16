using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        // TODO: rename this, it clashes with character feats
        public static class Feat
        {
            #pragma warning disable format

            public static readonly EntityPrototypeId DoorWooden    = new($"Elona.Feat{nameof(DoorWooden)}");
            public static readonly EntityPrototypeId DoorSF        = new($"Elona.Feat{nameof(DoorSF)}");
            public static readonly EntityPrototypeId DoorEastern   = new($"Elona.Feat{nameof(DoorEastern)}");
            public static readonly EntityPrototypeId DoorJail      = new($"Elona.Feat{nameof(DoorJail)}");
            public static readonly EntityPrototypeId Pot           = new($"Elona.Feat{nameof(Pot)}");
            public static readonly EntityPrototypeId HiddenPath    = new($"Elona.Feat{nameof(HiddenPath)}");
            public static readonly EntityPrototypeId QuestBoard    = new($"Elona.Feat{nameof(QuestBoard)}");
            public static readonly EntityPrototypeId VotingBox     = new($"Elona.Feat{nameof(VotingBox)}");
            public static readonly EntityPrototypeId SmallMedal    = new($"Elona.Feat{nameof(SmallMedal)}");
            public static readonly EntityPrototypeId PoliticsBoard = new($"Elona.Feat{nameof(PoliticsBoard)}");
            public static readonly EntityPrototypeId Mine          = new($"Elona.Feat{nameof(Mine)}");
            public static readonly EntityPrototypeId MapgenBlock   = new($"Elona.Feat{nameof(MapgenBlock)}");
            public static readonly EntityPrototypeId Plant         = new($"Elona.Feat{nameof(Plant)}");
            public static readonly EntityPrototypeId StairsDown    = new($"Elona.Feat{nameof(StairsDown)}");
            public static readonly EntityPrototypeId StairsUp      = new($"Elona.Feat{nameof(StairsUp)}");
            public static readonly EntityPrototypeId MapEntrance   = new($"Elona.Feat{nameof(MapEntrance)}");
            public static readonly EntityPrototypeId LockedHatch   = new($"Elona.Feat{nameof(LockedHatch)}");
            public static readonly EntityPrototypeId MaterialSpot  = new($"Elona.Feat{nameof(MaterialSpot)}");

            #pragma warning restore format
        }
    }
}

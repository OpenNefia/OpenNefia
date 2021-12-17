using EntityPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.Prototypes.EntityPrototype>;

namespace OpenNefia.Content.GameObjects
{
    public static partial class Protos
    {
        public static class Feat
        {
            public static readonly EntityPrototypeId Door = new($"Elona.FeatDoor");
            public static readonly EntityPrototypeId Pot = new($"Elona.FeatPot");
            public static readonly EntityPrototypeId HiddenPath = new($"Elona.FeatHiddenPath");
            public static readonly EntityPrototypeId QuestBoard = new($"Elona.FeatQuestBoard");
            public static readonly EntityPrototypeId VotingBox = new($"Elona.FeatVotingBox");
            public static readonly EntityPrototypeId SmallMedal = new($"Elona.FeatSmallMedal");
            public static readonly EntityPrototypeId PoliticsBoard = new($"Elona.FeatPoliticsBoard");
            public static readonly EntityPrototypeId Mine = new($"Elona.FeatMine");
            public static readonly EntityPrototypeId MapgenBlock = new($"Elona.FeatMapgenBlock");
            public static readonly EntityPrototypeId Plant = new($"Elona.FeatPlant");
            public static readonly EntityPrototypeId StairsDown = new($"Elona.FeatStairsDown");
            public static readonly EntityPrototypeId StairsUp = new($"Elona.FeatStairsUp");
            public static readonly EntityPrototypeId MapEntrance = new($"Elona.FeatMapEntrance");
            public static readonly EntityPrototypeId LockedHatch = new($"Elona.FeatLockedHatch");
            public static readonly EntityPrototypeId MaterialSpot = new($"Elona.FeatMaterialSpot");

        }
    }
}

using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.RandomGen
{
    public static class RandomGenConsts
    {
        public static class FilterSets
        {
            public static PrototypeId<TagPrototype> Dungeon(IRandom rand, IRandomGenSystem randomGen)
            {
                if (rand.OneIn(20))
                {
                    return randomGen.PickTag(Protos.TagSet.ItemRare);
                }
                if (rand.OneIn(3))
                {
                    return randomGen.PickTag(Protos.TagSet.ItemWear);
                }
                return randomGen.PickTag(Protos.TagSet.ItemItem);
            }
        }

        public static class ItemSets
        {
            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> Crop =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.Apple,
                    Protos.Item.Grape,
                    Protos.Item.Lemon,
                    Protos.Item.Strawberry,
                    Protos.Item.Cherry,
                    Protos.Item.Lettuce,
                    Protos.Item.Melon,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> Deed =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.Deed,
                    Protos.Item.DeedOfMuseum,
                    Protos.Item.DeedOfShop,
                    Protos.Item.DeedOfFarm,
                    Protos.Item.DeedOfStorageHouse,
                    Protos.Item.Shelter,
                    Protos.Item.DeedOfRanch,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> Fruit =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.Apple,
                    Protos.Item.Grape,
                    Protos.Item.Orange,
                    Protos.Item.Lemon,
                    Protos.Item.Strawberry,
                    Protos.Item.Cherry,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> GiftGrand =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.FiveHornedHelm,
                    Protos.Item.AuroraRing,
                    Protos.Item.SpeedRing,
                    Protos.Item.MauserC96Custom,
                    Protos.Item.Lightsabre,
                    Protos.Item.GouldsPiano,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> GiftMajor =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.KagamiMochi,
                    Protos.Item.KagamiMochi,
                    Protos.Item.Panty,
                    Protos.Item.BottleOfHermesBlood,
                    Protos.Item.ScrollOfSuperiorMaterial,
                    Protos.Item.FlyingScroll,
                    Protos.Item.SistersLoveFueledLunch,
                    Protos.Item.Shelter,
                    Protos.Item.SummoningCrystal,
                    Protos.Item.UnicornHorn,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> GiftMinor =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.Kotatsu,
                    Protos.Item.Daruma,
                    Protos.Item.Daruma,
                    Protos.Item.Mochi,
                    Protos.Item.Mochi,
                    Protos.Item.BlackCrystal,
                    Protos.Item.SnowMan,
                    Protos.Item.PaintingOfLandscape,
                    Protos.Item.PaintingOfSunflower,
                    Protos.Item.TreeOfFruits,
                    Protos.Item.TreasureBall,
                    Protos.Item.DeedOfHeirship,
                    Protos.Item.Rune,
                    Protos.Item.RemainsHeart,
                    Protos.Item.RemainsBlood,
                    Protos.Item.UprightPiano,
                    Protos.Item.DeadFish,
                    Protos.Item.Shit,
                    Protos.Item.SmallMedal,
                    Protos.Item.BrandNewGrave,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> Hire =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.BottleOfWhisky,
                    Protos.Item.PotionOfCureCriticalWound,
                    Protos.Item.PotionOfHealerOdina,
                    Protos.Item.RawOreOfEmerald,
                    Protos.Item.PotionOfCureMajorWound,
                    Protos.Item.PotionOfHealerJure,
                    Protos.Item.LongSword,
                    Protos.Item.LongSword,
                    Protos.Item.LongSword,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> ThrowPotionGreater =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.PotionOfConfusion,
                    Protos.Item.BottleOfWhisky,
                    Protos.Item.PotionOfSilence,
                    Protos.Item.PotionOfMutation,
                    Protos.Item.PotionOfWeakenResistance,
                    Protos.Item.PotionOfParalysis,
                    Protos.Item.Molotov,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> ThrowPotionMajor =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.PotionOfConfusion,
                    Protos.Item.PotionOfSlow,
                    Protos.Item.BottleOfWhisky,
                    Protos.Item.PotionOfSilence,
                    Protos.Item.PotionOfCureMutation,
                    Protos.Item.PotionOfWeakness,
                    Protos.Item.Molotov,
                    Protos.Item.Molotov,
                };

            public static readonly IReadOnlyList<PrototypeId<EntityPrototype>> ThrowPotionMinor =
                new List<PrototypeId<EntityPrototype>>() {
                    Protos.Item.PotionOfBlindness,
                    Protos.Item.PotionOfConfusion,
                    Protos.Item.PotionOfSlow,
                    Protos.Item.SleepingDrug,
                    Protos.Item.Poison,
                    Protos.Item.BottleOfBeer,
                    Protos.Item.PotionOfHero,
                    Protos.Item.BottleOfSulfuric,
                };
        }
    }
}

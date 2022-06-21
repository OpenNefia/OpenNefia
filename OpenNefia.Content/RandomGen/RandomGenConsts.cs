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
            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Armor =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipHead,
                    Protos.Tag.ItemCatEquipBody,
                    Protos.Tag.ItemCatEquipBack,
                    Protos.Tag.ItemCatEquipCloak,
                    Protos.Tag.ItemCatEquipLeg,
                    Protos.Tag.ItemCatEquipWrist,
                    Protos.Tag.ItemCatEquipShield,
                };

            // >>>>>>>> shade2/item.hsp:48 	fSetRemain=fltAmmo,fltFood,fltFood,fltOre,fltScro ...
            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Remain =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipAmmo,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatFood,
                };
            // <<<<<<<< shade2/item.hsp:48 	fSetRemain=fltAmmo,fltFood,fltFood,fltOre,fltScro ..

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Barrel =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipAmmo,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatJunkInField,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Chest =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipMelee,
                    Protos.Tag.ItemCatEquipRanged,
                    Protos.Tag.ItemCatEquipHead,
                    Protos.Tag.ItemCatEquipBody,
                    Protos.Tag.ItemCatEquipBack,
                    Protos.Tag.ItemCatEquipCloak,
                    Protos.Tag.ItemCatEquipLeg,
                    Protos.Tag.ItemCatEquipWrist,
                    Protos.Tag.ItemCatEquipShield,
                    Protos.Tag.ItemCatEquipRing,
                    Protos.Tag.ItemCatEquipNeck,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatMiscItem,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Collect =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatJunkInField,
                    Protos.Tag.ItemCatFurniture,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatOre,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Deliver =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatFurniture,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatJunk,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Income =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatRod,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFood,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Item =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatRod,
                    Protos.Tag.ItemCatGold,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatJunkInField,
                    Protos.Tag.ItemCatMiscItem,
                    Protos.Tag.ItemCatBook,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Magic =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatRod,
                    Protos.Tag.ItemCatSpellbook,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Perform =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFurniture,
                    Protos.Tag.ItemCatEquipLeg,
                    Protos.Tag.ItemCatEquipBack,
                    Protos.Tag.ItemCatEquipRing,
                    Protos.Tag.ItemCatEquipNeck,
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatJunkInField,
                    Protos.Tag.ItemCatJunkInField,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Plantartifact =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipRing,
                    Protos.Tag.ItemCatEquipNeck,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Plantunknown =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatFood,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatJunkInField,
                    Protos.Tag.ItemCatOre,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Rare =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatFurniture,
                    Protos.Tag.ItemCatContainer,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatBook,
                    Protos.Tag.ItemCatFood,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Rewardsupply =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatDrink,
                    Protos.Tag.ItemCatScroll,
                    Protos.Tag.ItemCatRod,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatFood,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Supply =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatFurniture,
                    Protos.Tag.ItemCatOre,
                    Protos.Tag.ItemCatRod,
                    Protos.Tag.ItemCatSpellbook,
                    Protos.Tag.ItemCatJunkInField,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Weapon =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipMelee,
                    Protos.Tag.ItemCatEquipRanged,
                    Protos.Tag.ItemCatEquipAmmo,
                };

            public static readonly IReadOnlyList<PrototypeId<TagPrototype>> Wear =
                new List<PrototypeId<TagPrototype>>() {
                    Protos.Tag.ItemCatEquipMelee,
                    Protos.Tag.ItemCatEquipMelee,
                    Protos.Tag.ItemCatEquipRanged,
                    Protos.Tag.ItemCatEquipRanged,
                    Protos.Tag.ItemCatEquipAmmo,
                    Protos.Tag.ItemCatEquipHead,
                    Protos.Tag.ItemCatEquipBody,
                    Protos.Tag.ItemCatEquipBack,
                    Protos.Tag.ItemCatEquipCloak,
                    Protos.Tag.ItemCatEquipLeg,
                    Protos.Tag.ItemCatEquipWrist,
                    Protos.Tag.ItemCatEquipShield,
                    Protos.Tag.ItemCatEquipRing,
                    Protos.Tag.ItemCatEquipNeck,
                };

            public static PrototypeId<TagPrototype> Dungeon(IRandom rand)
            {
                if (rand.OneIn(20))
                {
                    return rand.Pick(FilterSets.Rare);
                }
                if (rand.OneIn(3))
                {
                    return rand.Pick(FilterSets.Wear);
                }
                return rand.Pick(FilterSets.Item);
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

using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Items;
using OpenNefia.Content.Karma;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Wishes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Wishes
{
    [TestFixture]
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestOf(typeof(WishSystem))]
    public class WishSystem_Tests : OpenNefiaUnitTest
    {
        private MetaDataComponent? FindEntity(IEntityLookup lookup, IMap map, PrototypeId<EntityPrototype> id)
        {
            return lookup.EntityQueryInMap<MetaDataComponent>(map)
                .FirstOrDefault(e => e.EntityPrototype?.GetStrongID() == id);
        }

        [Test]
        [TestCase("en_US", "sex")]
        [TestCase("en_US", "gender")]
        [TestCase("ja_JP", "﻿性転換")]
        [TestCase("ja_JP", "﻿性転換")]
        [TestCase("ja_JP", "﻿性")]
        [TestCase("ja_JP", "﻿﻿異性")]
        public void TestWish_Sex(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;
            var gender = entMan.EnsureComponent<CharaComponent>(gameSession.Player);
            gender.Gender = Gender.Female;

            Assert.Multiple(() =>
            {
                Assert.That(gender.Gender, Is.EqualTo(Gender.Female));
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(gender.Gender, Is.EqualTo(Gender.Male));
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(gender.Gender, Is.EqualTo(Gender.Female));
            });
        }

        [Test]
        [TestCase("en_US", "friend")]
        [TestCase("en_US", "company")]
        [TestCase("en_US", "ally")]
        [TestCase("ja_JP", "﻿仲間")]
        public void TestWish_Ally(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;
            var deferred = sim.GetEntitySystem<IDeferredEventsSystem>();

            Assert.Multiple(() =>
            {
                Assert.That(deferred.IsEventEnqueued(), Is.False);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(deferred.IsEventEnqueued(), Is.True);
            });
        }

        [Test]
        [TestCase("en_US", "death")]
        [TestCase("ja_JP", "﻿死")]
        public void TestWish_Death(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(entMan.IsAlive(gameSession.Player), Is.True);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(entMan.IsAlive(gameSession.Player), Is.False);
            });
        }

        [Test]
        [TestCase("en_US", "money")]
        [TestCase("en_US", "gold")]
        [TestCase("en_US", "wealth")]
        [TestCase("en_US", "fortune")]
        [TestCase("ja_JP", "﻿金")]
        [TestCase("ja_JP", "﻿金貨")]
        [TestCase("ja_JP", "富")]
        [TestCase("ja_JP", "﻿財産")]
        public void TestWish_Gold(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(FindEntity(lookup, map, Protos.Item.GoldPiece), Is.Null);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(FindEntity(lookup, map, Protos.Item.GoldPiece), Is.Not.Null);
            });
        }

        [Test]
        [TestCase("en_US", "platina")]
        [TestCase("en_US", "platinum")]
        [TestCase("ja_JP", "﻿プラチナ")]
        [TestCase("ja_JP", "﻿プラチナ硬貨")]
        public void TestWish_Platinum(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(FindEntity(lookup, map, Protos.Item.PlatinumCoin), Is.Null);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(FindEntity(lookup, map, Protos.Item.PlatinumCoin), Is.Not.Null);
            });
        }

        [Test]
        [TestCase("en_US", "redemption")]
        [TestCase("en_US", "atonement")]
        [TestCase("ja_JP", "﻿贖罪")]
        public void TestWish_Redemption(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;
            var karma = entMan.EnsureComponent<KarmaComponent>(gameSession.Player);

            Assert.Multiple(() =>
            {
                karma.Karma.Base = -30;
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(karma.Karma.Base, Is.GreaterThan(-30)); // Randomized

                karma.Karma.Base = 20;
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(karma.Karma.Base, Is.EqualTo(20));
            });
        }

        [Test]
        [TestCase("en_US", "coin")]
        [TestCase("en_US", "medal")]
        [TestCase("en_US", "small coin")]
        [TestCase("en_US", "small medal")]
        [TestCase("ja_JP", "メダル")]
        [TestCase("ja_JP", "﻿小さなメダル")]
        [TestCase("ja_JP", "﻿ちいさなメダル")]
        public void TestWish_SmallMedal(string languageID, string wish)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    var lang = new PrototypeId<LanguagePrototype>(languageID);
                    factory.SwitchLanguage(lang);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(FindEntity(lookup, map, Protos.Item.SmallMedal), Is.Null);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(FindEntity(lookup, map, Protos.Item.SmallMedal), Is.Not.Null);
            });
        }

        private record class ItemWishTestCase(PrototypeId<LanguagePrototype> LangID, string Wish, PrototypeId<EntityPrototype> Expected);

        private static object[] ItemWishTestCases =
        {
           new object[] { LanguagePrototypeOf.English, "seven league boots", Protos.Item.SevenLeagueBoots },
           new object[] { LanguagePrototypeOf.English, "seven lea", Protos.Item.SevenLeagueBoots },
           new object[] { LanguagePrototypeOf.English, "item seven lea", Protos.Item.SevenLeagueBoots },
           new object[] { LanguagePrototypeOf.Japanese, "﻿セブンリーグブーツ", Protos.Item.SevenLeagueBoots },
           new object[] { LanguagePrototypeOf.Japanese, "アイテムセブンリーグ", Protos.Item.SevenLeagueBoots },

           // Name qualification
           new object[] { LanguagePrototypeOf.English, "potion of cure critical", Protos.Item.PotionOfCureCriticalWound },
           new object[] { LanguagePrototypeOf.Japanese, "﻿致命傷治癒のポーション", Protos.Item.PotionOfCureCriticalWound },
        };

        [Test]
        [TestCaseSource(nameof(ItemWishTestCases))]
        public void TestWish_Item(PrototypeId<LanguagePrototype> languageID, string wish, PrototypeId<EntityPrototype> itemID)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    factory.SwitchLanguage(languageID);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(FindEntity(lookup, map, itemID), Is.Null);
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(FindEntity(lookup, map, itemID), Is.Not.Null);
            });
        }

        private static object[] SkillWishTestCases =
        {
           new object []{LanguagePrototypeOf.English, "skill acid ground", Protos.Skill.SpellAcidGround },
           new object []{LanguagePrototypeOf.English, "skill charisma", Protos.Skill.AttrCharisma },
           new object []{LanguagePrototypeOf.English, "skill riding", Protos.Skill.Riding },
           new object []{LanguagePrototypeOf.Japanese, "スキル 酸の海", Protos.Skill.SpellAcidGround },
           new object []{LanguagePrototypeOf.Japanese, "スキル﻿ 魅力", Protos.Skill.AttrCharisma },
           new object []{LanguagePrototypeOf.Japanese, "スキル ﻿乗馬", Protos.Skill.Riding },
        };

        [Test]
        [TestCaseSource(nameof(SkillWishTestCases))]
        public void TestWish_Skill(PrototypeId<LanguagePrototype> languageID, string wish, PrototypeId<SkillPrototype> skillID)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizations(factory =>
                {
                    factory.SwitchLanguage(languageID);
                })
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();
            var skills = sim.GetEntitySystem<ISkillsSystem>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;
            var originalLevel = skills.BaseLevel(gameSession.Player, skillID);

            Assert.Multiple(() =>
            {
                Assert.That(wishes.GrantWish(wish), Is.True);
                Assert.That(skills.BaseLevel(gameSession.Player, skillID), Is.Not.EqualTo(originalLevel));
            });
        }

        [Test]
        public void TestWish_CardFigure()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(wishes.GrantWish("figure putit"), Is.True);
                var ent = FindEntity(lookup, map, Protos.Item.Figurine);
                Assert.That(ent, Is.Not.Null);
                Assert.That(entMan.EnsureComponent<EntityProtoSourceComponent>(ent!.Owner).EntityID, Is.EqualTo(Protos.Chara.Putit));

                Assert.That(wishes.GrantWish("card putit"), Is.True);
                ent = FindEntity(lookup, map, Protos.Item.Figurine);
                Assert.That(ent, Is.Not.Null);
                Assert.That(entMan.EnsureComponent<EntityProtoSourceComponent>(ent!.Owner).EntityID, Is.EqualTo(Protos.Chara.Putit));
            });
        }

        [Test]
        public void TestWish_WishAmount()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .LoadLocalizationsFromDisk()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var lookup = sim.GetEntitySystem<IEntityLookup>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var loc = sim.Resolve<ILocalizationManager>();

            var wishes = sim.GetEntitySystem<IWishSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            gameSession.Player = entGen.SpawnEntity(Protos.Chara.Android, map)!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(wishes.GrantWish("treasure map"), Is.True);
                var ent = FindEntity(lookup, map, Protos.Item.TreasureMap);
                Assert.That(ent, Is.Not.Null);
                Assert.That(entMan.EnsureComponent<StackComponent>(ent!.Owner).Count, Is.EqualTo(1));
            });
        }
    }
}
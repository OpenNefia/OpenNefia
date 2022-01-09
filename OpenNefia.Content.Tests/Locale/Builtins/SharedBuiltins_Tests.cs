using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Locale.Funcs;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Locale.Builtins
{
    [TestFixture]
    [TestOf(typeof(SharedBuiltins))]
    public class SharedBuiltins_Tests : ContentLocalizationUnitTest
    {
        protected override PrototypeId<LanguagePrototype> TestingLanguage => LanguagePrototypeOf.English;

        protected override ISimulationFactory GetSimulationFactory()
        {
            return base.GetSimulationFactory()
               .RegisterEntitySystems(factory =>
               {
                   factory.LoadExtraSystemType<VisibilitySystem>();
                   factory.LoadExtraSystemType<DisplayNameSystem>();
                   factory.LoadExtraSystemType<ObjectDisplayNameSystem>();
                   factory.LoadExtraSystemType<ItemNameSystem>();
               });
        }

        #region Builtin_name()

        private const string LocaleTestFile_name = @"
Elona.GameObjects.Common = {
    Something = 'something',
    You = 'you'
}

Test.Content.Builtins = {
    Name = function(arg)
        return ('%s'):format(_.name(arg))
    end,
}
";

        [Test]
        public void TestBuiltIn_name_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_name);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", 0)), Is.EqualTo("something"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", 99)), Is.EqualTo("something"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", 1.11111)), Is.EqualTo("something"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", 2.22222)), Is.EqualTo("something"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", true)), Is.EqualTo("something"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", "foo")), Is.EqualTo("something"));
        }

        [Test]
        public void TestBuiltIn_name_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_name);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            var entCharaOutOfSight = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(new Vector2i(20, 20)));

            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entCharaFemale)), Is.EqualTo("CharaFemale"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entCharaMale)), Is.EqualTo("CharaMale"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entCharaPlayer)), Is.EqualTo("you"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entCharaOutOfSight)), Is.EqualTo("something"));
        }

        [Test]
        public void TestBuiltIn_name_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_name);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entItemSingle)), Is.EqualTo("a ItemSingle"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Name", ("arg", entItemStacked)), Is.EqualTo("2 ItemStackeds"));
        }

        #endregion
    }
}

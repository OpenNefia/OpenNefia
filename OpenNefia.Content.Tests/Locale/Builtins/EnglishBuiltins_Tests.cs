using NUnit.Framework;
using OpenNefia.Content.Charas;
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
    [TestOf(typeof(EnglishBuiltins))]
    public class EnglishBuiltins_Tests : ContentLocalizationUnitTest
    {
        protected override PrototypeId<LanguagePrototype> TestingLanguage => LanguagePrototypeOf.English;

        #region Builtin_s()

        private const string LocaleTestFile_s = @"
Test.Content.Builtins = {
    S = function(arg, needE)
        return ('speak%s'):format(_.s(arg))
    end,
    Es = function(arg, needE)
        return ('wash%s'):format(_.s(arg, true))
    end
}
";

        [Test]
        public void TestBuiltIn_s_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_s);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", -99)), Is.EqualTo("speak"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 0)), Is.EqualTo("speak"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 1)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 2)), Is.EqualTo("speak"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 99)), Is.EqualTo("speak"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", 0)), Is.EqualTo("wash"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", 1)), Is.EqualTo("washes"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", 2)), Is.EqualTo("wash"));

            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 1.11111)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", 2.22222)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", true)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", "foo")), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", "foo")), Is.EqualTo("washes"));
        }

        [Test]
        public void TestBuiltIn_s_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_s);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", entCharaFemale)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", entCharaMale)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", entCharaPlayer)), Is.EqualTo("speak"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", entCharaFemale)), Is.EqualTo("washes"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", entCharaMale)), Is.EqualTo("washes"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", entCharaPlayer)), Is.EqualTo("wash"));
        }

        [Test]
        public void TestBuiltIn_s_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_s);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", entItemSingle)), Is.EqualTo("speaks"));
            Assert.That(locMan.GetString("Test.Content.Builtins.S", ("arg", entItemStacked)), Is.EqualTo("speak"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", entItemSingle)), Is.EqualTo("washes"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Es", ("arg", entItemStacked)), Is.EqualTo("wash"));
        }

        #endregion

        #region Builtin_has()

        private const string LocaleTestFile_has = @"
Test.Content.Builtins = {
    Has = function(arg)
        return ('%s'):format(_.has(arg))
    end,
}
";

        [Test]
        public void TestBuiltIn_has_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_has);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", -99)), Is.EqualTo("have"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 0)), Is.EqualTo("have"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 1)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 2)), Is.EqualTo("have"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 99)), Is.EqualTo("have"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 1.11111)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", 3.14)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", true)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", "foo")), Is.EqualTo("has"));
        }

        [Test]
        public void TestBuiltIn_has_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_has);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", entCharaFemale)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", entCharaMale)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", entCharaPlayer)), Is.EqualTo("have"));
        }

        [Test]
        public void TestBuiltIn_has_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_has);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", entItemSingle)), Is.EqualTo("has"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Has", ("arg", entItemStacked)), Is.EqualTo("have"));
        }

        #endregion
    }
}

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

        #region Builtin_is()

        private const string LocaleTestFile_is = @"
Test.Content.Builtins = {
    Is = function(arg)
        return ('%s'):format(_.is(arg))
    end,
}
";

        [Test]
        public void TestBuiltIn_is_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_is);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", -99)), Is.EqualTo("are"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 0)), Is.EqualTo("are"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 1)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 2)), Is.EqualTo("are"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 99)), Is.EqualTo("are"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 1.11111)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", 3.14)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", true)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", "foo")), Is.EqualTo("is"));
        }

        [Test]
        public void TestBuiltIn_is_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_is);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", entCharaFemale)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", entCharaMale)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", entCharaPlayer)), Is.EqualTo("are"));
        }

        [Test]
        public void TestBuiltIn_is_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_is);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", entItemSingle)), Is.EqualTo("is"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Is", ("arg", entItemStacked)), Is.EqualTo("are"));
        }

        #endregion

        #region Builtin_his()

        private const string LocaleTestFile_his = @"
Test.Content.Builtins = {
    His = function(arg)
        return ('%s'):format(_.his(arg))
    end,
}
";

        [Test]
        public void TestBuiltIn_his_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_his);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", -99)), Is.EqualTo("their"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 0)), Is.EqualTo("their"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 1)), Is.EqualTo("its"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 2)), Is.EqualTo("their"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 99)), Is.EqualTo("their"));

            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 1.11111)), Is.EqualTo("its"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", 3.14)), Is.EqualTo("its"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", true)), Is.EqualTo("its"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", "foo")), Is.EqualTo("its"));
        }

        [Test]
        public void TestBuiltIn_his_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_his);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", entCharaFemale)), Is.EqualTo("her"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", entCharaMale)), Is.EqualTo("his"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", entCharaPlayer)), Is.EqualTo("your"));
        }

        [Test]
        public void TestBuiltIn_his_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_his);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", entItemSingle)), Is.EqualTo("its"));
            Assert.That(locMan.GetString("Test.Content.Builtins.His", ("arg", entItemStacked)), Is.EqualTo("their"));
        }

        #endregion

        #region Builtin_him()

        private const string LocaleTestFile_him = @"
Test.Content.Builtins = {
    Him = function(arg)
        return ('%s'):format(_.him(arg))
    end,
}
";

        [Test]
        public void TestBuiltIn_him_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_him);
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", -99)), Is.EqualTo("them"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 0)), Is.EqualTo("them"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 1)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 2)), Is.EqualTo("them"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 99)), Is.EqualTo("them"));

            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 1.11111)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", 3.14)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", true)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", "foo")), Is.EqualTo("it"));
        }

        [Test]
        public void TestBuiltIn_him_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_him);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", entCharaFemale)), Is.EqualTo("her"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", entCharaMale)), Is.EqualTo("him"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", entCharaPlayer)), Is.EqualTo("you"));
        }

        [Test]
        public void TestBuiltIn_him_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_him);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", entItemSingle)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.Him", ("arg", entItemStacked)), Is.EqualTo("them"));
        }

        #endregion

        #region Builtin_theTarget()

        private const string LocaleTestFile_theTarget = @"
Test.Content.Builtins = {
    TheTarget = function(source, target)
        return ('%s'):format(_.theTarget(source, target))
    end,
}
";

        [Test]
        public void TestBuiltIn_theTarget_Primitives()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_theTarget);
            locMan.Resync();

            // Primitive boxing means that it will not output "itself".
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", 1), ("target", 1)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", 1), ("target", 2)), Is.EqualTo("it"));

            var obj1 = new object();
            var obj2 = new object();

            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", obj1), ("target", obj1)), Is.EqualTo("itself"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", obj2), ("target", obj1)), Is.EqualTo("it"));
        }

        [Test]
        public void TestBuiltIn_theTarget_Charas()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_theTarget);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var gameSession = sim.Resolve<IGameSessionManager>();
            var map = sim.ActiveMap!;

            var entCharaFemale = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            var entCharaMale = entMan.SpawnEntity(EntityCharaMaleID, map.AtPos(Vector2i.One));
            var entCharaPlayer = entMan.SpawnEntity(EntityCharaFemaleID, map.AtPos(Vector2i.One));
            gameSession.Player = entCharaPlayer;

            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaFemale), ("target", entCharaFemale)), Is.EqualTo("herself"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaMale), ("target", entCharaMale)), Is.EqualTo("himself"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaPlayer), ("target", entCharaPlayer)), Is.EqualTo("yourself"));

            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaPlayer), ("target", entCharaFemale)), Is.EqualTo("her"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaPlayer), ("target", entCharaMale)), Is.EqualTo("him"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entCharaMale), ("target", entCharaPlayer)), Is.EqualTo("you"));
        }

        [Test]
        public void TestBuiltIn_theTarget_Items()
        {
            var sim = SimulationFactory();

            var locMan = sim.Resolve<ILocalizationManager>();
            locMan.LoadString(LocaleTestFile_theTarget);
            locMan.Resync();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var entItemSingle = entMan.SpawnEntity(EntityItemSingleID, map.AtPos(Vector2i.One));
            var entItemStacked = entMan.SpawnEntity(EntityItemStackedID, map.AtPos(Vector2i.One));

            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entItemSingle), ("target", entItemSingle)), Is.EqualTo("itself"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entItemStacked), ("target", entItemStacked)), Is.EqualTo("themselves"));

            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entItemStacked), ("target", entItemSingle)), Is.EqualTo("it"));
            Assert.That(locMan.GetString("Test.Content.Builtins.TheTarget", ("source", entItemSingle), ("target", entItemStacked)), Is.EqualTo("them"));
        }

        #endregion
    }
}

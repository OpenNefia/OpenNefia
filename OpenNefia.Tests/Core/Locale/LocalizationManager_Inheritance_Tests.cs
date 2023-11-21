using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Tests.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Locale
{
    [TestFixture]
    [TestOf(typeof(LocalizationManager))]
    public class LocalizationManager_Inheritance_Tests : LocalizationUnitTest
    {
        private IPrototypeManager _protos = default!;

        const string DOCUMENT = @"
- type: Entity
  id: LocParent
- type: Entity
  parent: LocParent
  id: LocChild
";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();
            _protos = IoCManager.Resolve<IPrototypeManager>();
            _protos.RegisterType<EntityPrototype>();
            _protos.LoadString(DOCUMENT);
            _protos.ResolveResults();
        }

        [SetUp]
        public void Setup()
        {
            IoCManager.Resolve<IRandom>().PushSeed(0);
        }

        [Test]
        public void TestInheritance()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
OpenNefia.Prototypes.Entity = {
    LocParent = {
        Foo = 'Bar'
    },
}
");

            locMan.Resync();

            Assert.Multiple(() =>
            {
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocParent.Foo"), Is.EqualTo("Bar"));
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocChild.Foo"), Is.EqualTo("Bar"));
                Assert.That(locMan.TryGetTable("OpenNefia.Prototypes.Entity.LocParent", out var table), Is.True);
                Assert.That(locMan.TryGetTable("OpenNefia.Prototypes.Entity.LocChild", out table), Is.True);
            });
        }

        [Test]
        public void TestInheritance_NoOverwrite()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
OpenNefia.Prototypes.Entity = {
    LocParent = {
        Foo = 'Bar',
        Hoge = 'Piyo'
    },
    LocChild = {
        Foo = 'Baz'
    },
}
");

            locMan.Resync();

            Assert.Multiple(() =>
            {
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocParent.Foo"), Is.EqualTo("Bar"));
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocChild.Foo"), Is.EqualTo("Baz"));
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocChild.Hoge"), Is.EqualTo("<Missing key: OpenNefia.Prototypes.Entity.LocChild.Hoge>"));
            });
        }

        [Test]
        public void TestInheritance_NoOverwrite2()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
OpenNefia.Prototypes.Entity = {
    LocChild = {
        Foo = 'Baz'
    },
}
");

            locMan.Resync();

            Assert.Multiple(() =>
            {
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocParent.Foo"), Is.EqualTo("<Missing key: OpenNefia.Prototypes.Entity.LocParent.Foo>"));
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocChild.Foo"), Is.EqualTo("Baz"));
            });
        }

        [Test]
        public void TestInheritance_Missing()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.Resync();

            Assert.Multiple(() =>
            {
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocParent.Foo"), Is.EqualTo("<Missing key: OpenNefia.Prototypes.Entity.LocParent.Foo>"));
                Assert.That(locMan.GetString("OpenNefia.Prototypes.Entity.LocChild.Foo"), Is.EqualTo("<Missing key: OpenNefia.Prototypes.Entity.LocChild.Foo>"));
            });
        }
    }
}

using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Prototypes
{
    [TestFixture]
    [TestOf(typeof(PrototypeManager))]
    public class PrototypeManager_Ordering_Test : OpenNefiaUnitTest
    {
        private IPrototypeManager manager = default!;


        private static readonly PrototypeId<EntityPrototype> TestProto1ID = new("TestProto1");
        private static readonly PrototypeId<EntityPrototype> TestProto2ID = new("TestProto2");
        private static readonly PrototypeId<EntityPrototype> TestProto3ID = new("TestProto3");
        private static readonly PrototypeId<EntityPrototype> TestProto4ID = new("TestProto4");

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var factory = IoCManager.Resolve<IComponentFactory>();
            factory.RegisterClass<TestBasicPrototypeComponent>();

            IoCManager.Resolve<ISerializationManager>().Initialize();
            manager = IoCManager.Resolve<IPrototypeManager>();
        }

        [SetUp]
        public void Setup()
        {
            manager.Clear();
            manager.RegisterType<EntityPrototype>();
        }

        [Test]
        public void TestOrderingBefore()
        {
            var prototypes = @$"
- type: Entity
  id: {TestProto1ID}

- type: Entity
  id: {TestProto2ID}
  ordering:
    before: {TestProto1ID}
";

            manager.LoadString(prototypes);
            manager.Resync();

            var enumerator = manager.EnumeratePrototypes<EntityPrototype>()
                .Select(p => p.GetStrongID());

            Assert.That(enumerator, Is.EquivalentTo(new[] { TestProto2ID, TestProto1ID }));
        }

        [Test]
        public void TestOrderingAfter()
        {
            var prototypes = @$"
- type: Entity
  id: {TestProto1ID}
  ordering:
    after: {TestProto2ID}

- type: Entity
  id: {TestProto2ID}
";

            manager.LoadString(prototypes);
            manager.Resync();

            var enumerator = manager.EnumeratePrototypes<EntityPrototype>()
                .Select(p => p.GetStrongID());

            Assert.That(enumerator, Is.EquivalentTo(new[] { TestProto2ID, TestProto1ID }));
        }

        [Test]
        public void TestOrderingAutomatic()
        {
            var prototypes1 = @$"
- type: Entity
  id: {TestProto1ID}
  ordering:
    after: {TestProto4ID}

# Should be automatically ordered after TestProto1 above.
- type: Entity
  id: {TestProto2ID}
";

            var prototypes2 = @$"
- type: Entity
  id: {TestProto3ID}

# Should be automatically ordered after TestProto3 above.
- type: Entity
  id: {TestProto4ID}
";

            manager.LoadString(prototypes1);
            manager.LoadString(prototypes2);
            manager.Resync();

            var enumerator = manager.EnumeratePrototypes<EntityPrototype>()
                .Select(p => p.GetStrongID());

            Assert.That(enumerator, Is.EquivalentTo(new[] { TestProto3ID, TestProto4ID, TestProto1ID, TestProto2ID }));
        }
    }
}

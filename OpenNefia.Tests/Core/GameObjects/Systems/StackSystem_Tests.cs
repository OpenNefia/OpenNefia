using System;
using System.IO;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture]
    [TestOf(typeof(StackSystem))]
    public class StackSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> DummyID = new("dummy");

        private static readonly string Prototypes = @$"
- type: Entity
  name: dummy
  id: {DummyID}
  components:
  - type: Stack
";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void StackComponentLivenessTest()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            Assert.That(dummy, Is.Not.Null);
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);

            var stackableComp = entityManager.GetComponent<StackComponent>(dummy.Uid);

            Assert.That(stackableComp.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy.Uid, 0);

            Assert.That(stackableComp.Count, Is.EqualTo(0));
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.False);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.True);

            stackSys.SetCount(dummy.Uid, 1);

            Assert.That(stackableComp.Count, Is.EqualTo(1));
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);
        }

        [Test]
        public void StackComponentBoundingTest()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            Assert.That(dummy, Is.Not.Null);

            var stackableComp = entityManager.GetComponent<StackComponent>(dummy.Uid);

            Assert.That(stackableComp.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy.Uid, -5);
            Assert.That(stackableComp.Count, Is.EqualTo(0));

            stackSys.SetCount(dummy.Uid, 5);
            Assert.That(stackableComp.Count, Is.EqualTo(5));
        }
    }
}
using System;
using System.IO;
using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;

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
  - type: StackTest
    A: 42
    B: [1, 2, 3]
    C:
      foo: bar
      hoge: true
";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(compFac => {
                    compFac.RegisterClass<StackTestComponent>();
                })
                .RegisterDataDefinitionTypes(types => {
                    types.Add(typeof(StackTestComponent));
                    types.Add(typeof(StackTestNested));
                })
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        [Test]
        public void TestStackCountLiveness()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            Assert.That(dummy, Is.Not.Null);
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);

            var stack = entityManager.GetComponent<StackComponent>(dummy.Uid);

            Assert.That(stack.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy.Uid, 0);

            Assert.That(stack.Count, Is.EqualTo(0));
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.False);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.True);

            stackSys.SetCount(dummy.Uid, 1);

            Assert.That(stack.Count, Is.EqualTo(1));
            Assert.That(entityManager.IsAlive(dummy.Uid), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy.Uid), Is.False);
        }

        [Test]
        public void TestStackCountClamping()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy.Uid);

            Assert.That(stack.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy.Uid, -5);
            Assert.That(stack.Count, Is.EqualTo(0));

            stackSys.SetCount(dummy.Uid, 101);
            Assert.That(stack.Count, Is.EqualTo(101));
        }

        [Test]
        public void TestSplit_None()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy.Uid);

            stack.Count = 0;
            var mapCoords = map.AtPos(Vector2i.Zero);

            Assert.That(stackSys.TrySplit(dummy.Uid, 0, mapCoords, out var _), Is.False);
            Assert.That(stackSys.TrySplit(dummy.Uid, 1, mapCoords, out var _), Is.False);
        }

        [Test]
        public void TestSplit_One()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy.Uid);

            stack.Count = 1;
            var mapCoords = map.AtPos(Vector2i.Zero);
            var result = stackSys.TrySplit(dummy.Uid, 1, mapCoords, out var split);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True, "Result");
                Assert.That(split.IsValid(), Is.True, "Split entity IsValid()");
                Assert.That(entityManager.IsAlive(split), Is.True, "Split entity IsAlive()");
                Assert.That(entityManager.IsAlive(dummy.Uid), Is.False, "Original entity IsAlive()");
            });
        }

        [Test]
        public void TestSplit_Multiple()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy.Uid);

            stack.Count = 5;
            var mapCoords = map.AtPos(Vector2i.Zero);
            var result = stackSys.TrySplit(dummy.Uid, 3, mapCoords, out var split);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(split.IsValid(), Is.True);

                var splitSpatial = entityManager.GetComponent<SpatialComponent>(split);
                Assert.That(splitSpatial.MapPosition, Is.EqualTo(mapCoords));

                var splitStack = entityManager.GetComponent<StackComponent>(split);
                Assert.That(splitStack.Count, Is.EqualTo(3));
                Assert.That(stack.Count, Is.EqualTo(2));

                var splitStackTest = entityManager.GetComponent<StackTestComponent>(split);
                Assert.That(splitStackTest.A, Is.EqualTo(42));
                Assert.That(splitStackTest.B, Is.EquivalentTo(new List<int> { 1, 2, 3 }));
                Assert.That(splitStackTest.C.Foo, Is.EqualTo("bar"));
                Assert.That(splitStackTest.C.Hoge, Is.EqualTo(true));
            });
        }

        [Test]
        public void TestStack_Success()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy1 = entityManager.CreateEntityUninitialized(DummyID);
            var dummy2 = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy1.Uid);

            stack.Count = 5;
            var result = stackSys.TryStack(dummy1.Uid, dummy2.Uid);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(entityManager.IsAlive(dummy1.Uid), Is.True);
                Assert.That(entityManager.IsAlive(dummy2.Uid), Is.False);
                Assert.That(stack.Count, Is.EqualTo(6));
            });
        }

        [Test]
        public void TestStack_Invalid()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy1 = entityManager.CreateEntityUninitialized(DummyID);
            var dummy2 = entityManager.CreateEntityUninitialized(DummyID);

            Assert.That(stackSys.TryStack(dummy1.Uid, EntityUid.Invalid), Is.False);
            Assert.That(stackSys.TryStack(dummy1.Uid, dummy1.Uid), Is.False);

            entityManager.DeleteEntity(dummy2);

            Assert.That(stackSys.TryStack(dummy1.Uid, dummy2.Uid), Is.False);
        }

        [Test]
        public void TestStack_NotSame()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy1 = entityManager.CreateEntityUninitialized(DummyID);
            var dummy2 = entityManager.CreateEntityUninitialized(DummyID);

            var stackTest = entityManager.GetComponent<StackTestComponent>(dummy2.Uid);
            stackTest.A = 9999;

            Assert.That(stackSys.TryStack(dummy1.Uid, dummy2.Uid), Is.False);
        }
    }

    public class StackTestComponent : Component
    {
        public override string Name => "StackTest";

        [DataField("A")]
        public int A { get; set; }

        [DataField("B")]
        public List<int> B { get; set; } = new();

        [DataField("C")]
        public StackTestNested C { get; set; } = new();
    }

    [DataDefinition]
    public class StackTestNested 
    {
        [DataField]
        public string Foo { get; set; } = string.Empty;

        [DataField]
        public bool Hoge { get; set; }
    }
}
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
        private static readonly PrototypeId<EntityPrototype> DummyNonStackedID = new("dummyNonStacked");

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

- type: Entity
  name: dummy
  id: {DummyNonStackedID}
  components:
  - type: StackTest
";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterComponents(compFac =>
                {
                    compFac.RegisterClass<StackTestComponent>();
                })
                .RegisterDataDefinitionTypes(types =>
                {
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
            Assert.That(entityManager.IsAlive(dummy), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy), Is.False);

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            Assert.That(stack.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy, 0);

            Assert.That(stack.Count, Is.EqualTo(0));
            Assert.That(entityManager.IsAlive(dummy), Is.False);
            Assert.That(entityManager.IsDeadAndBuried(dummy), Is.True);

            stackSys.SetCount(dummy, 1);

            Assert.That(stack.Count, Is.EqualTo(1));
            Assert.That(entityManager.IsAlive(dummy), Is.True);
            Assert.That(entityManager.IsDeadAndBuried(dummy), Is.False);
        }

        [Test]
        public void TestStackCountClamping()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            Assert.That(stack.Count, Is.EqualTo(1));

            stackSys.SetCount(dummy, -5);
            Assert.That(stack.Count, Is.EqualTo(0));

            stackSys.SetCount(dummy, 101);
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

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            stack.Count = 0;
            var mapCoords = map.AtPos(Vector2i.Zero);

            Assert.That(stackSys.TrySplit(dummy, 0, mapCoords, out var _), Is.False);
            Assert.That(stackSys.TrySplit(dummy, 1, mapCoords, out var _), Is.False);
        }

        /// <summary>
        /// Splitting a stack size of one should just return the original entity, to prevent
        /// unneeded entity allocation.
        /// </summary>
        [Test]
        public void TestSplit_One()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            stack.Count = 1;
            var mapCoords = map.AtPos(Vector2i.Zero);
            var result = stackSys.TrySplit(dummy, 1, mapCoords, out var split);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True, "Result");
                Assert.That(entityManager.IsAlive(split), Is.True, "Split entity IsAlive()");
                Assert.That(entityManager.IsAlive(dummy), Is.True, "Original entity IsAlive()");
                Assert.That(split, Is.EqualTo(dummy), "Split entity equal to original entity");
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

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            stack.Count = 5;
            var mapCoords = map.AtPos(Vector2i.Zero);
            var result = stackSys.TrySplit(dummy, 3, mapCoords, out var split);

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
        public void TestSplit_SameAmount()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyID);

            var stack = entityManager.GetComponent<StackComponent>(dummy);

            stack.Count = 5;
            var mapCoords = map.AtPos(Vector2i.Zero);

            Assert.Multiple(() =>
            {
                Assert.That(stackSys.TrySplit(dummy, 5, mapCoords, out var split), Is.True);
                Assert.That(split, Is.EqualTo(dummy));

                Assert.That(stackSys.TrySplit(dummy, 6, mapCoords, out _), Is.False);
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

            Assert.That(stackSys.TryStack(EntityUid.Invalid, EntityUid.Invalid), Is.False);
            Assert.That(stackSys.TryStack(dummy1, EntityUid.Invalid), Is.False);
            Assert.That(stackSys.TryStack(dummy1, dummy1), Is.False);

            entityManager.DeleteEntity(dummy2);

            Assert.That(stackSys.TryStack(dummy1, dummy2), Is.False);
        }

        [Test]
        public void TestStack_NotSame()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();

            var dummy1 = entityManager.CreateEntityUninitialized(DummyID);
            var dummy2 = entityManager.CreateEntityUninitialized(DummyID);

            var stackTest = entityManager.GetComponent<StackTestComponent>(dummy2);
            stackTest.A = 9999;

            Assert.That(stackSys.TryStack(dummy1, dummy2), Is.False);
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

            var stack = entityManager.GetComponent<StackComponent>(dummy1);

            stack.Count = 5;
            var result = stackSys.TryStack(dummy1, dummy2);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(entityManager.IsAlive(dummy1), Is.True);
                Assert.That(entityManager.IsAlive(dummy2), Is.False);
                Assert.That(stack.Count, Is.EqualTo(6));
            });
        }

        /// <summary>
        /// If an entity doesn't have a StackComponent, allow splitting off the
        /// stack if and only if a stack of size 1 is requested. In this case, 
        /// the split entity is the same as the input entity.
        /// </summary>
        [Test]
        public void TestSplit_NonStacked()
        {
            var sim = SimulationFactory();
            var entityManager = sim.Resolve<IEntityManager>();
            var stackSys = sim.GetEntitySystem<IStackSystem>();
            var map = sim.ActiveMap!;

            var dummy = entityManager.CreateEntityUninitialized(DummyNonStackedID);
            var mapCoords = map.AtPos(Vector2i.Zero);

            Assert.That(stackSys.TrySplit(dummy, 2, mapCoords, out var _), Is.False);

            var result = stackSys.TrySplit(dummy, 1, mapCoords, out var split);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True, "Result");
                Assert.That(split.IsValid(), Is.True, "Split entity IsValid()");
                Assert.That(entityManager.IsAlive(split), Is.True, "Split entity IsAlive()");
                Assert.That(entityManager.IsAlive(dummy), Is.True, "Original entity IsAlive()");
                Assert.That(dummy, Is.EqualTo(split), "Original entity is equal to split entity");
            });
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
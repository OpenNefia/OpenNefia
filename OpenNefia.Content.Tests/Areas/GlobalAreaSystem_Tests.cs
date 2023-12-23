using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture]
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestOf(typeof(GlobalAreaSystem))]
    public class GlobalAreaSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly GlobalAreaId TestArea1ID = new("TestArea1");
        private static readonly GlobalAreaId TestArea2ID = new("TestArea2");
        private static readonly GlobalAreaId TestArea3ID = new("TestArea3");
        private static readonly GlobalAreaId TestArea4ID = new("TestArea4");
        private static readonly GlobalAreaId TestArea5ID = new("TestArea5");

        private static readonly string Prototypes = $@"
- type: Entity
  id: TestArea1
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea1ID}

- type: Entity
  id: TestArea2
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea2ID}
      parent: {TestArea1ID}

- type: Entity
  id: TestArea3
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea3ID}
      parent: {TestArea1ID}

- type: Entity
  id: TestArea4
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea4ID}
      parent: {TestArea2ID}

- type: Entity
  id: TestArea5
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea5ID}
";

        [Test]
        public void TestGlobalAreaInit_Acyclic()
        {

            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();
            var areaManager = sim.Resolve<IAreaManager>();

            globalAreas.InitializeGlobalAreas(new[] { TestArea1ID, TestArea2ID, TestArea3ID, TestArea4ID, TestArea5ID });

            Assert.Multiple(() =>
            {
                Assert.That(areaManager.TryGetGlobalArea(TestArea2ID, out var area), Is.True);
                Assert.That(area!.GlobalId, Is.EqualTo(TestArea2ID));
                Assert.That(areaManager.TryGetParentArea(area!.Id, out var parent), Is.True);
                Assert.That(parent!.GlobalId, Is.EqualTo(TestArea1ID));

                Assert.That(areaManager.TryGetGlobalArea(TestArea3ID, out area), Is.True);
                Assert.That(area!.GlobalId, Is.EqualTo(TestArea3ID));
                Assert.That(areaManager.TryGetParentArea(area!.Id, out parent), Is.True);
                Assert.That(parent!.GlobalId, Is.EqualTo(TestArea1ID));

                Assert.That(areaManager.TryGetGlobalArea(TestArea1ID, out area), Is.True);
                Assert.That(areaManager.TryGetParentArea(area!.Id, out parent), Is.False);
            });
        }

        [Test]
        public void TestGlobalAreas_EnumerateGlobalAreaTree()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();
            var areaManager = sim.Resolve<IAreaManager>();

            globalAreas.InitializeGlobalAreas(new[] { TestArea1ID, TestArea2ID, TestArea3ID, TestArea4ID, TestArea5ID });

            Assert.Multiple(() =>
            {
                Assert.That(globalAreas.EnumerateGlobalAreaTree(TestArea1ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea1ID, TestArea2ID, TestArea3ID, TestArea4ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaTree(TestArea2ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea2ID, TestArea4ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaTree(TestArea3ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea3ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaTree(TestArea4ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea4ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaTree(TestArea5ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea5ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaTree(new("Foo")),
                    Is.EquivalentTo(new GlobalAreaId[] { }));
            });
        }

        [Test]
        public void TestGlobalAreas_EnumerateGlobalAreaParents()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();
            var areaManager = sim.Resolve<IAreaManager>();

            globalAreas.InitializeGlobalAreas(new[] { TestArea1ID, TestArea2ID, TestArea3ID, TestArea4ID, TestArea5ID });

            Assert.Multiple(() =>
            {
                Assert.That(globalAreas.EnumerateGlobalAreaParents(TestArea1ID),
                    Is.EquivalentTo(new GlobalAreaId[] { }));
                Assert.That(globalAreas.EnumerateGlobalAreaParents(TestArea2ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea1ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaParents(TestArea3ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea1ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaParents(TestArea4ID),
                    Is.EquivalentTo(new GlobalAreaId[] { TestArea2ID, TestArea1ID }));
                Assert.That(globalAreas.EnumerateGlobalAreaParents(TestArea5ID),
                    Is.EquivalentTo(new GlobalAreaId[] { }));
                Assert.That(globalAreas.EnumerateGlobalAreaParents(new("Foo")),
                    Is.EquivalentTo(new GlobalAreaId[] { }));
            });
        }

        [Test]
        public void TestGlobalAreas_GetRootGlobalArea()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();
            var areaManager = sim.Resolve<IAreaManager>();

            globalAreas.InitializeGlobalAreas(new[] { TestArea1ID, TestArea2ID, TestArea3ID, TestArea4ID, TestArea5ID });

            Assert.Multiple(() =>
            {
                Assert.That(globalAreas.GetRootGlobalArea(TestArea1ID), Is.EqualTo(TestArea1ID));
                Assert.That(globalAreas.GetRootGlobalArea(TestArea2ID), Is.EqualTo(TestArea1ID));
                Assert.That(globalAreas.GetRootGlobalArea(TestArea3ID), Is.EqualTo(TestArea1ID));
                Assert.That(globalAreas.GetRootGlobalArea(TestArea4ID), Is.EqualTo(TestArea1ID));
                Assert.That(globalAreas.GetRootGlobalArea(TestArea5ID), Is.EqualTo(TestArea5ID));
                Assert.That(globalAreas.GetRootGlobalArea(new("Foo")), Is.Null);
            });
        }

        [Test]
        public void TestGlobalAreaInit_DuplicateID()
        {
            string prototypes = $@"
- type: Entity
  id: TestArea1
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea1ID}

- type: Entity
  id: TestArea2
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea1ID}
";


            Assert.Throws<InvalidDataException>(() =>
            {
                var sim = ContentFullGameSimulation
                    .NewSimulation()
                    .RegisterPrototypes(protos => protos.LoadString(prototypes))
                    .InitializeInstance();

                var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();

                globalAreas.InitializeGlobalAreas(new[] { TestArea1ID });
            });
        }

        [Test]
        public void TestGlobalAreaInit_Cyclic()
        {
            string Prototypes = $@"
- type: Entity
  id: TestArea1
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea1ID}
      parent: {TestArea2ID}

- type: Entity
  id: TestArea2
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea2ID}
      parent: {TestArea1ID}
";


            Assert.Throws<InvalidDataException>(() =>
            {
                var sim = ContentFullGameSimulation
                    .NewSimulation()
                    .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                    .InitializeInstance();

                var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();

                globalAreas.InitializeGlobalAreas(new[] { TestArea1ID });
            });
        }

        [Test]
        public void TestGlobalAreaInit_MissingParent()
        {
            string prototypes = $@"
- type: Entity
  id: TestArea1
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    globalArea:
      id: {TestArea1ID}
      parent: {TestArea2ID}
";

            Assert.Throws<InvalidDataException>(() =>
            {
                var sim = ContentFullGameSimulation
                    .NewSimulation()
                    .RegisterPrototypes(protos => protos.LoadString(prototypes))
                    .InitializeInstance();

                var globalAreas = sim.GetEntitySystem<IGlobalAreaSystem>();

                globalAreas.InitializeGlobalAreas(new[] { TestArea1ID });
            });
        }
    }
}

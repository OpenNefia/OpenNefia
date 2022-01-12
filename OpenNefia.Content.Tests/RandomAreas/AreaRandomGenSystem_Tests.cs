using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(AreaRandomGenSystem))]
    public class AreaRandomGenSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestAreaEntityID = new("TestAreaEntity");

        private static readonly AreaFloorId TestAreaFloorID = new("Test.AreaFloor");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestAreaEntityID}
  parent: BaseArea
  components:
  - type: TestRandomArea
";

        [Test]
        public void TestAreaRandomGen_IsActive()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterComponents(factory => factory.RegisterClass<TestRandomAreaComponent>())
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var areaRandomGen = sim.GetEntitySystem<AreaRandomGenSystem>();

            var area = areaMan.CreateArea(TestAreaEntityID);

            var testComp = entMan.GetComponent<TestRandomAreaComponent>(area.AreaEntityUid);

            testComp.IsActive = true;
            Assert.That(areaRandomGen.IsRandomAreaActive(area), Is.True);

            testComp.IsActive = false;
            Assert.That(areaRandomGen.IsRandomAreaActive(area), Is.False);
        }

        [Test]
        public void TestAreaRandomGen_Regenerate()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterComponents(factory => factory.RegisterClass<TestRandomAreaComponent>())
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var areaRandomGen = sim.GetEntitySystem<AreaRandomGenSystem>();

            var otherMap = mapMan.CreateMap(10, 10);
            var mapWithAreas = mapMan.CreateMap(100, 100);
            var mapRandomAreaMan = entMan.EnsureComponent<MapRandomAreaManagerComponent>(mapWithAreas.MapEntityUid);
            mapRandomAreaMan.RegenerateRandomAreas = false;

            mapMan.SetActiveMap(mapWithAreas.Id);

            Assert.That(areaRandomGen.GetTotalActiveRandomAreasInMap(mapWithAreas.Id), Is.EqualTo(0));

            // First check that no areas are generated on ungeneratable tiles.
            mapRandomAreaMan.RegenerateRandomAreas = true;
            mapWithAreas.Clear(Protos.Tile.Empty);

            mapMan.SetActiveMap(otherMap.Id);
            mapMan.SetActiveMap(mapWithAreas.Id);

            Assert.That(mapRandomAreaMan.RegenerateRandomAreas, Is.False);
            Assert.That(areaRandomGen.GetTotalActiveRandomAreasInMap(mapWithAreas.Id), Is.EqualTo(0));

            // Now make all the tiles possible candidates to generate on.
            mapRandomAreaMan.RegenerateRandomAreas = true;
            mapWithAreas.Clear(AreaRandomGenSystem.RandomAreaSpawnableTiles.First());

            mapMan.SetActiveMap(otherMap.Id);
            mapMan.SetActiveMap(mapWithAreas.Id);

            Assert.That(mapRandomAreaMan.RegenerateRandomAreas, Is.False);
            Assert.That(areaRandomGen.GetTotalActiveRandomAreasInMap(mapWithAreas.Id), Is.EqualTo(AreaRandomGenSystem.ActiveRandomAreaThreshold));
        }

        private class TestRandomAreaSystem : EntitySystem
        {
            [Dependency] private readonly IMapManager _mapManager = default!;
            [Dependency] private readonly IAreaManager _areaManager = default!;
            [Dependency] private readonly IAreaEntranceSystem _areaEntrances = default!;

            public override void Initialize()
            {
                SubscribeLocalEvent<TestRandomAreaComponent, RandomAreaCheckIsActiveEvent>(HandleIsActive, nameof(HandleIsActive));
                SubscribeLocalEvent<GenerateRandomAreaEvent>(GenerateRandomArea, nameof(GenerateRandomArea));
            }

            private void HandleIsActive(EntityUid uid, TestRandomAreaComponent component, RandomAreaCheckIsActiveEvent args)
            {
                args.IsActive |= component.IsActive;
            }

            private void GenerateRandomArea(GenerateRandomAreaEvent args)
            {
                if (args.Handled)
                    return;

                if (!_mapManager.TryGetMap(args.RandomAreaCoords.MapId, out var map))
                    return;

                AreaId? parentAreaId = null;
                if (_areaManager.TryGetAreaOfMap(map, out var parentArea))
                    parentAreaId = parentArea.Id;

                var area = _areaManager.CreateArea(TestAreaEntityID, parent: parentAreaId);
                var testRandomAreaComp = EntityManager.GetComponent<TestRandomAreaComponent>(area.AreaEntityUid);
                testRandomAreaComp.IsActive = true;

                args.Handle(area);
            }
        }

        [ComponentUsage(ComponentTarget.Area)]
        private class TestRandomAreaComponent : Component
        {
            public override string Name => "TestRandomArea";
        
            [DataField]
            public bool IsActive { get; set; }
        }
    }
}

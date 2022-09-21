using Elfie.Serialization;
using Microsoft.VisualStudio.CodeCoverage;
using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(AreaKnownEntrancesSystem))]
    public class AreaKnownEntrancesSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly GlobalAreaId TestArea1ID = new("TestArea1");

        private static readonly string Prototypes = $@"
- type: Entity
  id: TestArea1
  parent: BaseArea
  components:
  - type: AreaBlankMap
  - type: AreaEntrance
    startingFloor: Test:0
    globalId: {TestArea1ID}
";

        private ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            sim.GetEntitySystem<IGlobalAreaSystem>().InitializeGlobalAreas();

            return sim;
        }

        [Test]
        public void TestAreaKnownEntrancesSystem_Creation()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var entranceSys = sim.GetEntitySystem<IAreaEntranceSystem>();
            var sys = sim.GetEntitySystem<IAreaKnownEntrancesSystem>();

            var worldMap = sim.CreateMapAndSetActive(10, 10);

            var area1 = areaMan.GetGlobalArea(TestArea1ID);
            var area1Map = areaMan.GetOrGenerateMapForFloor(area1.Id, new("Test", 0), worldMap.AtPos(0, 0))!;

            Assert.That(sys.EnumerateKnownEntrancesTo(area1Map), Is.Empty);

            // Test creation
            var entrance1Pos = worldMap.AtPos(1, 1);
            var entrance1 = entranceSys.CreateAreaEntrance(area1, entrance1Pos);
            var entrance1Spatial = entMan.GetComponent<SpatialComponent>(entrance1.Owner);

            Assert.Multiple(() =>
            {
                var known = sys.EnumerateKnownEntrancesTo(area1Map);
                Assert.That(known.Count, Is.EqualTo(1));

                var meta = known.First();
                Assert.That(meta.EntranceEntity, Is.EqualTo(entrance1.Owner));
                Assert.That(meta.MapCoordinates, Is.EqualTo(entrance1Pos));
            });
        }

        [Test]
        public void TestAreaKnownEntrancesSystem_Movement()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var entranceSys = sim.GetEntitySystem<IAreaEntranceSystem>();
            var sys = sim.GetEntitySystem<IAreaKnownEntrancesSystem>();

            var worldMap = sim.CreateMapAndSetActive(10, 10);

            var area1 = areaMan.GetGlobalArea(TestArea1ID);
            var area1Map = areaMan.GetOrGenerateMapForFloor(area1.Id, new("Test", 0), worldMap.AtPos(0, 0))!;

            var entrance1Pos = worldMap.AtPos(1, 1);
            var entrance1 = entranceSys.CreateAreaEntrance(area1, entrance1Pos);
            var entrance1Spatial = entMan.GetComponent<SpatialComponent>(entrance1.Owner);

            entrance1Spatial.Coordinates = worldMap.AtPosEntity(2, 2);

            Assert.Multiple(() =>
            {
                var known = sys.EnumerateKnownEntrancesTo(area1Map);
                Assert.That(known.Count, Is.EqualTo(1));

                var meta = known.First();
                Assert.That(meta.EntranceEntity, Is.EqualTo(entrance1.Owner));
                Assert.That(meta.MapCoordinates, Is.EqualTo(worldMap.AtPos(2, 2)));
            });

        }

        [Test]
        public void TestAreaKnownEntrancesSystem_MultipleCreation()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var entranceSys = sim.GetEntitySystem<IAreaEntranceSystem>();
            var sys = sim.GetEntitySystem<IAreaKnownEntrancesSystem>();

            var worldMap = sim.CreateMapAndSetActive(10, 10);

            var area1 = areaMan.GetGlobalArea(TestArea1ID);
            var area1Map = areaMan.GetOrGenerateMapForFloor(area1.Id, new("Test", 0), worldMap.AtPos(0, 0))!;

            Assert.That(sys.EnumerateKnownEntrancesTo(area1Map), Is.Empty);

            var entrance1Pos = worldMap.AtPos(1, 1);
            var entrance1 = entranceSys.CreateAreaEntrance(area1, entrance1Pos);
            var entrance1Spatial = entMan.GetComponent<SpatialComponent>(entrance1.Owner);
            var entrance2Pos = worldMap.AtPos(2, 2);
            var entrance2 = entranceSys.CreateAreaEntrance(area1, entrance2Pos);

            Assert.That(sys.EnumerateKnownEntrancesTo(area1Map).Count, Is.EqualTo(2));
        }

        [Test]
        public void TestAreaKnownEntrancesSystem_Deletion()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var entranceSys = sim.GetEntitySystem<IAreaEntranceSystem>();
            var sys = sim.GetEntitySystem<IAreaKnownEntrancesSystem>();

            var worldMap = sim.CreateMapAndSetActive(10, 10);

            var area1 = areaMan.GetGlobalArea(TestArea1ID);
            var area1Map = areaMan.GetOrGenerateMapForFloor(area1.Id, new("Test", 0), worldMap.AtPos(0, 0))!;

            var entrance1Pos = worldMap.AtPos(1, 1);
            var entrance1 = entranceSys.CreateAreaEntrance(area1, entrance1Pos);
            var entrance1Spatial = entMan.GetComponent<SpatialComponent>(entrance1.Owner);

            Assert.Multiple(() =>
            {
                var known = sys.EnumerateKnownEntrancesTo(area1Map);
                Assert.That(known.Count, Is.EqualTo(1));

                var meta = known.First();
                Assert.That(meta.EntranceEntity, Is.EqualTo(entrance1.Owner));
                Assert.That(meta.MapCoordinates, Is.EqualTo(entrance1Pos));
            });

            entMan.DeleteEntity(entrance1.Owner);

            Assert.That(sys.EnumerateKnownEntrancesTo(area1Map), Is.Empty);
        }

        [Test]
        public void TestAreaKnownEntrancesSystem_UnloadMap()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var entranceSys = sim.GetEntitySystem<IAreaEntranceSystem>();
            var sys = sim.GetEntitySystem<IAreaKnownEntrancesSystem>();

            var worldMap = sim.CreateMapAndSetActive(10, 10);

            var area1 = areaMan.GetGlobalArea(TestArea1ID);
            var area1Map = areaMan.GetOrGenerateMapForFloor(area1.Id, new("Test", 0), worldMap.AtPos(0, 0))!;
            var area1MapId = area1Map.Id;

            var entrance1Pos = worldMap.AtPos(1, 1);
            var entrance1 = entranceSys.CreateAreaEntrance(area1, entrance1Pos);
            var entrance1Spatial = entMan.GetComponent<SpatialComponent>(entrance1.Owner);

            Assert.That(sys.EnumerateKnownEntrancesTo(area1MapId).Count, Is.EqualTo(1));

            mapMan.UnloadMap(area1MapId);

            Assert.That(sys.EnumerateKnownEntrancesTo(area1MapId).Count, Is.EqualTo(1));
        }
    }
}
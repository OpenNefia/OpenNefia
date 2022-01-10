using NUnit.Framework;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Areas
{
    [TestFixture]
    [TestOf(typeof(AreaManager))]
    public class AreaManager_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<MapPrototype> TestMapID = new("TestMap");
        private static readonly PrototypeId<AreaPrototype> TestAreaID = new("TestArea");
        private static readonly AreaFloorId TestMapFloor = new("Test.Map");

        private static readonly string Prototypes = @$"
- type: Map
  id: {TestMapID}
  blueprintPath: /Maps/Test/test.yml

- type: Area
  id: {TestAreaID}
  initialFloors:
    {TestMapFloor}: {TestMapID}
  startingFloor: {TestMapFloor}
";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            protoMan.RegisterType<MapPrototype>();
            protoMan.RegisterType<AreaPrototype>();
            protoMan.LoadString(Prototypes);
            protoMan.Resync();
        }

        [SetUp]
        public void Setup()
        {
            var mapMan = IoCManager.Resolve<IMapManagerInternal>();
            
            mapMan.FlushMaps();
            IoCManager.Resolve<IAreaManagerInternal>().FlushAreas();
            IoCManager.Resolve<IEntityManagerInternal>().FlushEntities();

            mapMan.CreateMap(1, 1, MapId.Global);
        }

        [Test]
        public void TestRegisterArea()
        {
            var areaMan = IoCManager.Resolve<IAreaManagerInternal>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            areaMan.NextAreaId = 2;

            var area = new Area();
            var areaId = new AreaId(42);
            var areaEnt = entMan.CreateEntityUninitialized(null);

            areaMan.RegisterArea(area, areaId, areaEnt);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.AreaEntityUid, Is.EqualTo(areaEnt));
            Assert.That(area.Id, Is.EqualTo(areaId));

            // RegisterArea() shouldn't affect the next area ID (it's used
            // for deserialization).
            Assert.That(areaMan.NextAreaId, Is.EqualTo(2));
        }

        [Test]
        public void TestCreateArea()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var area = areaMan.CreateArea();

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.ContainedMaps.Count, Is.EqualTo(0));

            var areaComp = entMan.GetComponent<AreaComponent>(area.AreaEntityUid);
            Assert.That(areaComp.AreaId, Is.EqualTo(area.Id));
            Assert.That(areaComp.StartingFloor, Is.Null);

            var areaSpatialComp = entMan.GetComponent<SpatialComponent>(area.AreaEntityUid);
            Assert.That(areaSpatialComp.MapID, Is.EqualTo(MapId.Global));

            var globalMapEnt = mapMan.GetMap(MapId.Global).MapEntityUid;
            Assert.That(areaSpatialComp.ParentUid, Is.EqualTo(globalMapEnt));
        }

        [Test]
        public void TestCreateArea_FromPrototype()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            var area = areaMan.CreateArea(TestAreaID);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.ContainedMaps.Count, Is.EqualTo(1));
            Assert.That(area.ContainedMaps[TestMapFloor].MapId, Is.Null);
            Assert.That(area.ContainedMaps[TestMapFloor].DefaultGenerator, Is.EqualTo(TestMapID));
        }

        [Test]
        public void TestCreateArea_HighestAreaID()
        {
            var areaMan = IoCManager.Resolve<IAreaManagerInternal>();

            areaMan.CreateArea();
            var area2 = areaMan.CreateArea();

            Assert.That(areaMan.NextAreaId == (int)area2.Id + 1);
        }

        [Test]
        public void TestRegisterAreaFloor()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var area = areaMan.CreateArea();
            var map = mapMan.CreateMap(10, 10);
            var floorId = TestMapFloor;

            areaMan.RegisterAreaFloor(area, floorId, map);

            Assert.That(area.ContainedMaps[floorId].MapId, Is.EqualTo(map.Id));
            Assert.That(area.ContainedMaps[floorId].DefaultGenerator, Is.EqualTo(new PrototypeId<MapPrototype>("Blank")));

            Assert.Throws<ArgumentException>(() => areaMan.RegisterAreaFloor(area, floorId, map));
        }

        [Test]
        public void TestUnregisterAreaFloor()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var area = areaMan.CreateArea();
            var map = mapMan.CreateMap(10, 10);
            var floorId = TestMapFloor;

            Assert.Throws<ArgumentException>(() => areaMan.UnregisterAreaFloor(area, floorId));

            ((Area)area)._containedMaps[floorId] = new AreaFloor(map.Id);

            Assert.That(area.ContainedMaps.ContainsKey(floorId), Is.True);

            areaMan.UnregisterAreaFloor(area, floorId);

            Assert.That(area.ContainedMaps.ContainsKey(floorId), Is.False);
        }
    }
}
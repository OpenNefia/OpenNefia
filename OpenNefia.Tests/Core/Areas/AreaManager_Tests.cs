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
        private static readonly PrototypeId<EntityPrototype> TestAreaID = new("TestArea");
        private static readonly AreaFloorId TestMapFloor = new("Test.Map", 0);

        private static readonly string Prototypes = @$"
- type: Entity
  id: {TestAreaID}
";

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            IoCManager.Resolve<ISerializationManager>().Initialize();

            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            protoMan.RegisterType<MapPrototype>();
            protoMan.RegisterType<EntityPrototype>();
            protoMan.LoadString(Prototypes);
            protoMan.ResolveResults();
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
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            areaMan.NextAreaId = 2;

            var area = new Area();
            var areaId = new AreaId(42);
            var areaEnt = entMan.CreateEntityUninitialized(null);

            // Simulate a registered area floor being loaded directly from disk
            // (no RegisterAreaFloor() was called)
            var map = mapMan.CreateMap(10, 10);
            area._containedMaps[TestMapFloor] = new AreaFloor(map.Id);
            
            areaMan.RegisterArea(area, areaId, areaEnt);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.AreaEntityUid, Is.EqualTo(areaEnt));
            Assert.That(area.Id, Is.EqualTo(areaId));

            //Assert.That(areaMan.TryGetAreaAndFloorOfMap(map.Id, out var foundArea, out var foundFloor), Is.True);
            //Assert.That(foundArea, Is.EqualTo(area));
            //Assert.That(foundFloor, Is.EqualTo(TestMapFloor));

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

            var area = areaMan.CreateArea(null);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.ContainedMaps.Count, Is.EqualTo(0));

            var areaComp = entMan.GetComponent<AreaComponent>(area.AreaEntityUid);
            Assert.That(areaComp.AreaId, Is.EqualTo(area.Id));

            var areaSpatialComp = entMan.GetComponent<SpatialComponent>(area.AreaEntityUid);
            Assert.That(areaSpatialComp.MapID, Is.EqualTo(MapId.Global));

            var globalMapEnt = mapMan.GetMap(MapId.Global).MapEntityUid;
            Assert.That(areaSpatialComp.ParentUid, Is.EqualTo(globalMapEnt));
        }

        [Test]
        public void TestCreateArea_FromPrototype_Invalid()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            Assert.Throws<ArgumentException>(() => areaMan.CreateArea(areaEntityProtoId: new("Dood")));
        }

        [Test]
        public void TestCreateArea_FromPrototype()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            var area = areaMan.CreateArea(TestAreaID);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.ContainedMaps.Count, Is.EqualTo(0));
            Assert.That(area.GlobalId, Is.Null);
        }

        [Test]
        public void TestCreateArea_WithGlobalID()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            var globalId = new GlobalAreaId("Test.Area");
            var area = areaMan.CreateArea(null, globalId: globalId);

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(areaMan.GlobalAreaExists(globalId), Is.True);

            var found = areaMan.GetGlobalArea(globalId);
            Assert.That(found.GlobalId, Is.EqualTo(globalId));

            Assert.Throws<ArgumentException>(() => areaMan.CreateArea(null, globalId: globalId));
        }

        [Test]
        public void TestCreateArea_HighestAreaID()
        {
            var areaMan = IoCManager.Resolve<IAreaManagerInternal>();

            areaMan.CreateArea(null);
            var area2 = areaMan.CreateArea(null);

            Assert.That(areaMan.NextAreaId == (int)area2.Id + 1);
        }

        [Test]
        public void TestCreateArea_Parenting()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            var parentArea = areaMan.CreateArea(null);
            var area = areaMan.CreateArea(null, parent: parentArea.Id);

            Assert.That(areaMan.TryGetParentArea(area.Id, out var parentArea2), Is.True);
            Assert.That(parentArea2, Is.Not.Null);
            Assert.That(parentArea2!.Id, Is.EqualTo(parentArea.Id));
        }

        [Test]
        public void TestRegisterAreaFloor()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var area = areaMan.CreateArea(null);
            var map = mapMan.CreateMap(10, 10);
            var floorId = TestMapFloor;

            areaMan.RegisterAreaFloor(area, floorId, map);

            Assert.That(area.ContainedMaps[floorId].MapId, Is.EqualTo(map.Id));
            Assert.That(areaMan.TryGetAreaAndFloorOfMap(map.Id, out var foundArea, out var foundFloor), Is.True);
            Assert.That(foundArea, Is.EqualTo(area));
            Assert.That(foundFloor, Is.EqualTo(floorId));

            Assert.Throws<ArgumentException>(() => areaMan.RegisterAreaFloor(area, floorId, map), "Attempting to reuse same area floor ID");

            var floorId2 = new AreaFloorId("Test.Floor2", 2);
            Assert.Throws<ArgumentException>(() => areaMan.RegisterAreaFloor(area, floorId2, map), "Attempting to register same map with two different floors");

            var area2 = areaMan.CreateArea(null);
            Assert.Throws<ArgumentException>(() => areaMan.RegisterAreaFloor(area2, floorId, map), "Attempting to register same map with two different areas");
        }

        [Test]
        public void TestUnregisterAreaFloor()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();
            var mapMan = IoCManager.Resolve<IMapManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();

            var area = areaMan.CreateArea(null);
            var map = mapMan.CreateMap(10, 10);
            var floorId = TestMapFloor;

            Assert.Throws<ArgumentException>(() => areaMan.UnregisterAreaFloor(area, floorId));

            ((Area)area)._containedMaps[floorId] = new AreaFloor(map.Id);

            Assert.That(area.ContainedMaps.ContainsKey(floorId), Is.True);

            areaMan.UnregisterAreaFloor(area, floorId);

            Assert.That(area.ContainedMaps.ContainsKey(floorId), Is.False);
            Assert.That(areaMan.TryGetAreaAndFloorOfMap(map.Id, out _, out _), Is.False);
        }
    }
}
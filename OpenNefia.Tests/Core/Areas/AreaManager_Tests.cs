using NUnit.Framework;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
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
        [SetUp]
        public void Setup()
        {
            IoCManager.Resolve<IMapManagerInternal>().FlushMaps();
            IoCManager.Resolve<IAreaManagerInternal>().FlushAreas();
            IoCManager.Resolve<IEntityManagerInternal>().FlushEntities();
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
            // for deserialization)
            Assert.That(areaMan.NextAreaId, Is.EqualTo(2));
        }

        [Test]
        public void TestCreateArea()
        {
            var areaMan = IoCManager.Resolve<IAreaManager>();

            var area = areaMan.CreateArea();

            Assert.That(areaMan.AreaExists(area.Id), Is.True);
            Assert.That(area.ContainedMaps.Count, Is.EqualTo(0));
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
            var floorId = new AreaFloorId("Test.FloorID");

            areaMan.RegisterAreaFloor(area, floorId, map);

            Assert.That(area.ContainedMaps[floorId].MapId, Is.EqualTo(map.Id));
            Assert.That(area.ContainedMaps[floorId].DefaultGenerator, Is.Null);

            var areaComp = entMan.GetComponent<AreaComponent>(area.AreaEntityUid);
            Assert.That(areaComp.AreaId, Is.EqualTo(area.Id));
            Assert.That(areaComp.StartingFloor, Is.Null);

            var areaSpatialComp = entMan.GetComponent<SpatialComponent>(area.AreaEntityUid);
            Assert.That(areaSpatialComp.Mapless, Is.True);
            Assert.That(areaSpatialComp.Parent, Is.Null);
            Assert.That(areaSpatialComp.MapID, Is.EqualTo(MapId.Nullspace));

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
            var floorId = new AreaFloorId("Test.FloorID");

            Assert.Throws<ArgumentException>(() => areaMan.UnregisterAreaFloor(area, floorId));

            ((Area)area)._containedMaps[floorId] = new AreaFloor(map.Id);

            areaMan.UnregisterAreaFloor(area, floorId);

            Assert.That(area.ContainedMaps.ContainsKey(floorId), Is.False);
        }
    }
}
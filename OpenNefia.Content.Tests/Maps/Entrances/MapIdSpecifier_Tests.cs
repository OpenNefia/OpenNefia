using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Maps.Entrances
{
    [TestFixture]
    public class MapIdSpecifier_Tests
    {
        [Test]
        [TestOf(typeof(NullMapIdSpecifier))]
        public void TestNullMapIdSpecifier()
        {
            var mapIdSpec = new NullMapIdSpecifier();

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(null));
        }

        [Test]
        [TestOf(typeof(BasicMapIdSpecifier))]
        public void TestBasicMapIdSpecifier()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var mapMan = sim.Resolve<IMapManager>();

            var map = mapMan.CreateMap(10, 10);

            var mapIdSpec = new BasicMapIdSpecifier(map.Id);

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(map.Id));
        }

        [Test]
        [TestOf(typeof(AreaFloorMapIdSpecifier))]
        public void TestAreaFloorMapIdSpecifier()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entMan = sim.Resolve<IEntityManager>();

            var map1 = mapMan.CreateMap(10, 10);
            var map2 = mapMan.CreateMap(10, 10);

            var area = areaMan.CreateArea();
            var areaFloorId1 = new AreaFloorId("Test.Floor1");
            var areaFloorId2 = new AreaFloorId("Test.Floor2");
            areaMan.RegisterAreaFloor(area, areaFloorId1, map1);
            areaMan.RegisterAreaFloor(area, areaFloorId2, map2);
            var areaDefEntrance = entMan.EnsureComponent<AreaEntranceComponent>(area.AreaEntityUid);
            areaDefEntrance.StartingFloor = areaFloorId1;

            var mapIdSpec = new AreaFloorMapIdSpecifier(area.Id);

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(map1.Id));

            mapIdSpec = new AreaFloorMapIdSpecifier(area.Id, areaFloorId2);

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(map2.Id));
        }

        [Test]
        [TestOf(typeof(GlobalAreaMapIdSpecifier))]
        public void TestGlobalAreaMapIdSpecifier()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var entMan = sim.Resolve<IEntityManager>();

            var map1 = mapMan.CreateMap(10, 10);
            var map2 = mapMan.CreateMap(10, 10);

            var globalId = new GlobalAreaId("Test.GlobalArea");

            var area = areaMan.CreateArea(globalId: globalId);
            var areaFloorId1 = new AreaFloorId("Test.Floor1");
            var areaFloorId2 = new AreaFloorId("Test.Floor2");
            areaMan.RegisterAreaFloor(area, areaFloorId1, map1);
            areaMan.RegisterAreaFloor(area, areaFloorId2, map2);
            var areaDefEntrance = entMan.EnsureComponent<AreaEntranceComponent>(area.AreaEntityUid);
            areaDefEntrance.StartingFloor = areaFloorId1;

            var mapIdSpec = new GlobalAreaMapIdSpecifier(globalId);

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(map1.Id));
            Assert.That(mapIdSpec.ResolvedAreaId, Is.EqualTo(area.Id));

            mapIdSpec = new GlobalAreaMapIdSpecifier(globalId, areaFloorId2);

            Assert.That(mapIdSpec.GetMapId(), Is.EqualTo(map2.Id));
            Assert.That(mapIdSpec.ResolvedAreaId, Is.EqualTo(area.Id));
        }
    }
}

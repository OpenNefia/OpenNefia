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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(AreaEntranceSystem))]
    public class AreaEntranceSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestAreaEntityID = new("TestAreaEntity");
        private static readonly PrototypeId<EntityPrototype> TestEntranceEntityID = new("TestEntranceEntity");

        private static readonly AreaFloorId TestAreaFloorID = new("Test.AreaFloor");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestAreaEntityID}
  parent: BaseArea
  components:
  - type: AreaEntrance
    entranceEntity: {TestEntranceEntityID}
    startLocation: !type:SpecificMapLocation
      pos: 1,1
    startingFloor: {TestAreaFloorID}

- type: Entity
  id: {TestEntranceEntityID}
  parent: {Protos.Mobj.MapEntrance}
  components:
  - type: Chip
    id: {Protos.Chip.MObjAreaBorderTent}
";

        [Test]
        public void TestCreateAreaEntrance()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var areaEntranceSys = sim.GetEntitySystem<AreaEntranceSystem>();

            var map = mapMan.CreateMap(50, 50);
            var area = areaMan.CreateArea(TestAreaEntityID);

            areaMan.RegisterAreaFloor(area, TestAreaFloorID, map);

            mapMan.SetActiveMap(map.Id);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            var worldMapEntComp = areaEntranceSys.CreateAreaEntrance(area, map.AtPos(new Vector2i(5, 5)));

            Assert.That(worldMapEntComp.Entrance.MapIdSpecifier.GetMapId(), Is.EqualTo(map.Id));
            Assert.That(worldMapEntComp.Entrance.MapIdSpecifier.GetAreaId(), Is.EqualTo(area.Id));
            Assert.That(worldMapEntComp.Entrance.StartLocation, Is.TypeOf(typeof(SpecificMapLocation)));

            var worldMapEntMeta = entMan.GetComponent<MetaDataComponent>(worldMapEntComp.Owner);
            Assert.That(worldMapEntMeta.EntityPrototype?.GetStrongID(), Is.EqualTo(TestEntranceEntityID));

            var worldMapEntChip = entMan.GetComponent<ChipComponent>(worldMapEntComp.Owner);
            Assert.That(worldMapEntChip.ChipID, Is.EqualTo(Protos.Chip.MObjAreaBorderTent));
        }
    }
}

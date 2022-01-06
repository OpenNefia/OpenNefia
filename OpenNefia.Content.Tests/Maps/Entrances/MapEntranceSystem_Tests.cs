using NUnit.Framework;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Maps.Entrances
{
    [TestFixture]
    [TestOf(typeof(MapEntranceSystem))]
    public class MapEntranceSystem_Tests
    {
        [Test]
        public void TestMapTransfer()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entranceSys = sim.GetEntitySystem<MapEntranceSystem>();

            var map1 = mapMan.CreateMap(50, 50);
            var map2 = mapMan.CreateMap(50, 50);

            mapMan.SetActiveMap(map1.Id);

            var ent = entMan.SpawnEntity(null, map1.AtPos(Vector2i.One));

            var startPos = new Vector2i(12, 34);

            var entrance = new MapEntrance()
            {
                DestinationMapId = map2.Id,
                StartLocation = new SpecificMapLocation(startPos)
            };

            var result = entranceSys.UseMapEntrance(ent, entrance);

            Assert.That(result, Is.EqualTo(TurnResult.Succeeded));

            var entSpatial = entMan.GetComponent<SpatialComponent>(ent);
            Assert.That(entSpatial.MapPosition, Is.EqualTo(map2.AtPos(12, 34)));
        }
    }
}
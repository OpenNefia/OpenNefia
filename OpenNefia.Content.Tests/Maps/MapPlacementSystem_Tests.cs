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
    [TestFixture, Parallelizable]
    [TestOf(typeof(MapPlacementSystem))]
    public class MapPlacementSystem_Tests
    {
        [Test]
        public void TestMapPlacement_Blocked()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapPlacement = sim.GetEntitySystem<IMapPlacement>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var desired = map.AtPos(Vector2i.One);

            var ent = entMan.SpawnEntity(null, desired);
            entMan.GetComponent<SpatialComponent>(ent).IsSolid = true;

            Assert.That(mapPlacement.FindFreePositionForChara(desired), Is.EqualTo(map.AtPos((1, 0))));
        }
        
        [Test]
        public void TestMapPlacement_Open()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapPlacement = sim.GetEntitySystem<IMapPlacement>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var desired = map.AtPos(Vector2i.One);

            var ent = entMan.SpawnEntity(null, desired);
            entMan.GetComponent<SpatialComponent>(ent).IsSolid = false;

            Assert.That(mapPlacement.FindFreePositionForChara(desired), Is.EqualTo(desired));
        }
    }
}
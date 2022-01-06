using NUnit.Framework;
using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests.Maps.StartLocation
{
    [TestFixture]
    [TestOf(typeof(MapEdgesLocation))]
    public class MapEdgesLocation_Tests
    {
        private static object[] MapEdgesLocationDefault_Cases =
        {
           new object[] { new Vector2i(10, 10), Direction.North, new Vector2i(5, 8) },
           new object[] { new Vector2i(10, 10), Direction.NorthWest, new Vector2i(5, 8) },
           new object[] { new Vector2i(10, 10), Direction.NorthEast, new Vector2i(5, 8) },
           new object[] { new Vector2i(10, 10), Direction.South, new Vector2i(5, 1) },
           new object[] { new Vector2i(10, 10), Direction.SouthWest, new Vector2i(5, 1) },
           new object[] { new Vector2i(10, 10), Direction.SouthEast, new Vector2i(5, 1) },
           new object[] { new Vector2i(10, 10), Direction.East, new Vector2i(1, 5) },
           new object[] { new Vector2i(10, 10), Direction.West, new Vector2i(8, 5) },
           new object[] { new Vector2i(10, 10), Direction.Invalid, new Vector2i(5, 5) },
        };

        [Test]
        [TestCaseSource(nameof(MapEdgesLocationDefault_Cases))]
        public void TestMapEdgesLocationDefault(Vector2i mapSize, Direction direction, Vector2i expectedPos)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();

            var map = sim.CreateMapAndSetActive(mapSize.X, mapSize.Y);
            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            var entSpatial = entMan.GetComponent<SpatialComponent>(ent);

            entSpatial.Direction = direction;

            var loc = new MapEdgesLocation();
            Assert.That(loc.GetStartPosition(ent, map), Is.EqualTo(expectedPos));
        }

        /// <summary>
        /// Tests that when entering a map using a <see cref="MapEdgesLocation"/>,
        /// the entities with <see cref="MapEdgesLocationComponent"/> in the map
        /// are referenced.
        /// </summary>
        [Test]
        public void TestMapEdgesLocationSpecified()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            var direction = Direction.North;
            var expectedPos = new Vector2i(3, 4);

            var posEnt = entMan.SpawnEntity(null, map.AtPos(expectedPos));
            var posComp = entMan.EnsureComponent<MapEdgesLocationComponent>(posEnt);
            posComp.TargetDirection = direction;

            var entSpatial = entMan.GetComponent<SpatialComponent>(ent);

            entSpatial.Direction = direction;

            var loc = new MapEdgesLocation();
            Assert.That(loc.GetStartPosition(ent, map), Is.EqualTo(expectedPos));
        }
    }
}

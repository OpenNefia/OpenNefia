using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Maps
{
    [TestFixture]
    [TestOf(typeof(EntityLookup))]
    public class EntityLookup_Tests : OpenNefiaUnitTest
    {
        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                   .NewSimulation()
                   .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        /// <summary>
        /// When an entity is created, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshSpatialIndexOnEntityCreation()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var lookup = sim.GetEntitySystem<IEntityLookup>();

            var coords = map.AtPos(Vector2i.One);

            Assert.That(lookup.GetLiveEntitiesAtCoords(coords), Is.EquivalentTo(new SpatialComponent[0]));

            var ent = entMan.SpawnEntity(null, coords);
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.That(lookup.GetLiveEntitiesAtCoords(coords), Is.EquivalentTo(new SpatialComponent[] { spatial }));
        }

        /// <summary>
        /// When an entity is moved, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshSpatialIndexOnEntityMoved()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var lookup = sim.GetEntitySystem<IEntityLookup>();

            var coords1 = map.AtPos(Vector2i.One);
            var coords2 = coords1.Offset(Vector2i.One);
            var ent = entMan.SpawnEntity(null, coords1);
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.Multiple(() =>
            {
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords1), Is.EquivalentTo(new SpatialComponent[] { spatial }));
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords2), Is.EquivalentTo(new SpatialComponent[0]));
            });

            spatial.WorldPosition = coords2.Position;

            Assert.Multiple(() =>
            {
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords1), Is.EquivalentTo(new SpatialComponent[0]));
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords2), Is.EquivalentTo(new SpatialComponent[] { spatial }));
            });

            spatial.WorldPosition = coords1.Position;

            Assert.Multiple(() =>
            {
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords1), Is.EquivalentTo(new SpatialComponent[] { spatial }));
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords2), Is.EquivalentTo(new SpatialComponent[0]));
            });
        }

        /// <summary>
        /// When an entity is deleted, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshSpatialIndexOnEntityDeleted()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var lookup = sim.GetEntitySystem<IEntityLookup>();

            var coords = map.AtPos(Vector2i.One);
            var ent = entMan.SpawnEntity(null, coords);
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.That(lookup.GetLiveEntitiesAtCoords(coords), Is.EquivalentTo(new SpatialComponent[] { spatial }));

            entMan.DeleteEntity(ent);

            Assert.That(lookup.GetLiveEntitiesAtCoords(coords), Is.EquivalentTo(new SpatialComponent[0]));
        }

        [Test]
        public void TestMoveBetweenMapsRefresh()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();

            var map1 = sim.ActiveMap!;
            var map2 = sim.Resolve<IMapManager>().CreateMap(50, 50);

            var lookup = sim.GetEntitySystem<IEntityLookup>();

            var coords1 = new EntityCoordinates(map1.MapEntityUid, Vector2i.Zero);
            var coords2 = new EntityCoordinates(map2.MapEntityUid, Vector2i.Zero);

            var ent = entMan.SpawnEntity(null, coords1);
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.Multiple(() =>
            {
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords1), Is.EquivalentTo(new SpatialComponent[] { spatial }));
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords2), Is.EquivalentTo(new SpatialComponent[0]));
            });

            spatial.Coordinates = coords2;

            Assert.Multiple(() =>
            {
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords1), Is.EquivalentTo(new SpatialComponent[0]));
                Assert.That(lookup.GetLiveEntitiesAtCoords(coords2), Is.EquivalentTo(new SpatialComponent[] { spatial }));
            });
        }

        [Test]
        public void TestGetLiveEntitiesAtPos()
        {
            var simulation = SimulationFactory();
            var entMan = simulation.Resolve<IEntityManager>();
            var entSysMan = simulation.Resolve<IEntitySystemManager>();
            var lookup = entSysMan.GetEntitySystem<IEntityLookup>();

            var pos = Vector2i.Zero;

            var map = simulation.ActiveMap!;
            var entMap = map.MapEntityUid;

            // "Alive" means the entity is considered the primary entity on the tile.
            // In HSP Elona, each map tile can only hold a single character ID for
            // positional querying purposes; this emulates that behavior.
            var entAlive = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaAlive = entMan.GetComponent<MetaDataComponent>(entAlive);
            metaAlive.Liveness = EntityGameLiveness.Alive;

            // Child entities don't count as being in the map.
            var entChild = entMan.SpawnEntity(null, new EntityCoordinates(entAlive, Vector2i.Zero));
            var metaChild = entMan.GetComponent<MetaDataComponent>(entChild);
            metaChild.Liveness = EntityGameLiveness.Alive;

            // "Hidden" means the entity is not visible in the map and cannot be targeted,
            // but should not be removed from the map, such as for dead allies.
            var entHidden = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaHidden = entMan.GetComponent<MetaDataComponent>(entHidden);
            metaHidden.Liveness = EntityGameLiveness.Hidden;

            // "DeadAndBuried" means the entity can be removed at any time.
            var entDead = entMan.SpawnEntity(null, map.AtPos(pos));
            var metaDead = entMan.GetComponent<MetaDataComponent>(entDead);
            metaDead.Liveness = EntityGameLiveness.DeadAndBuried;

            Assert.Multiple(() =>
            {
                var ents = lookup.GetAllEntitiesIn(map.Id)
                                 .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive,
                    entChild,
                    entHidden,
                    entDead
                }), "All Entities");

                ents = lookup.GetEntitiesDirectlyIn(map.Id)
                             .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive,
                    entHidden,
                    entDead
                }), "Entities Directly In Map");

                ents = lookup.GetLiveEntitiesAtCoords(map.AtPos(pos))
                             .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive
                }), "Entities At Coords");
            });

            Assert.Multiple(() =>
            {
                var ents = lookup.GetAllEntitiesIn(map.Id, includeMapEntity: true)
                                 .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive,
                    entChild,
                    entHidden,
                    entDead,
                    entMap
                }), "All Entities (including parent)");

                ents = lookup.GetEntitiesDirectlyIn(map.Id, includeMapEntity: true)
                             .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive,
                    entHidden,
                    entDead,
                    entMap
                }), "Entities Directly In Map (including parent)");

                ents = lookup.GetLiveEntitiesAtCoords(map.AtPos(pos), includeMapEntity: true)
                             .Select(spatial => spatial.Owner);

                Assert.That(ents, Is.EquivalentTo(new EntityUid[]
                {
                    entAlive,
                    entMap
                }), "Entities At Coords (including parent)");
            });
        }
    }
}

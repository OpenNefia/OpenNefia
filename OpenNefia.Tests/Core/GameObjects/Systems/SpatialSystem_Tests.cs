using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(SpatialSystem))]
    class SpatialSystem_Tests
    {
        private static readonly PrototypeId<EntityPrototype> IdDummySolidOpaque = new("dummySolidOpaque");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {IdDummySolidOpaque}
  components:
  - type: Spatial
    isSolid: true
    isOpaque: true";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        /// <summary>
        /// When an entity is spawned, a PositionChangedEvent is raised.
        /// </summary>
        [Test]
        public void TestRaiseMoveOnSpawnEvent()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var subscriber = new Subscriber();
            int calledCount = 0;
            var entUid = new { Uid = EntityUid.Invalid };
            entMan.EventBus.SubscribeEvent<EntityPositionChangedEvent>(subscriber, MoveEventHandler);
            var ent1 = entMan.SpawnEntity(null, new MapCoordinates(map.Id, Vector2i.Zero));

            Assert.That(calledCount, Is.EqualTo(1));
            Assert.That(entUid.Uid, Is.EqualTo(ent1));
            void MoveEventHandler(ref EntityPositionChangedEvent ev)
            {
                calledCount++;
                // OldPosition has the entity UID of the newly created entity.
                entUid = entUid with { Uid = ev.OldPosition.EntityId };
                Assert.That(ev.OldPosition.Position, Is.EqualTo(Vector2i.Zero));
                Assert.That(ev.NewPosition, Is.EqualTo(new EntityCoordinates(mapEnt, Vector2i.Zero)));
            }
        }

        /// <summary>
        /// When the local position of the spatial component changes, an EntityPositionChangedEvent is raised.
        /// </summary>
        [Test]
        public void TestRaiseMoveEvent()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var subscriber = new Subscriber();
            int calledCount = 0;
            var ent = entMan.SpawnEntity(null, new MapCoordinates(map.Id, Vector2i.Zero));
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            entMan.EventBus.SubscribeEvent<EntityPositionChangedEvent>(subscriber, MoveEventHandler);

            spatial.WorldPosition = Vector2i.One;

            Assert.That(calledCount, Is.EqualTo(1));
            void MoveEventHandler(ref EntityPositionChangedEvent ev)
            {
                calledCount++;
                Assert.That(ev.OldPosition, Is.EqualTo(new EntityCoordinates(mapEnt, Vector2i.Zero)));
                Assert.That(ev.NewPosition, Is.EqualTo(new EntityCoordinates(mapEnt, Vector2i.One)));
            }
        }

        /// <summary>
        /// When an entity is created, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshMapTileFlagsOnEntityCreation()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var pos = Vector2i.One;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);

            entMan.SpawnEntity(IdDummySolidOpaque, map.AtPos(pos));

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);
        }

        /// <summary>
        /// When an entity is moved, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshMapTileFlagsOnEntityMoved()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var pos1 = Vector2i.One;
            var pos2 = pos1 + Vector2i.One;

            var ent = entMan.SpawnEntity(IdDummySolidOpaque, map.AtPos(pos1));
            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.That(map.CanAccess(pos1), Is.False);
            Assert.That(map.CanSeeThrough(pos1), Is.False);
            Assert.That(map.CanAccess(pos2), Is.True);
            Assert.That(map.CanSeeThrough(pos2), Is.True);

            spatial.WorldPosition = pos2;

            Assert.That(map.CanAccess(pos1), Is.True);
            Assert.That(map.CanSeeThrough(pos1), Is.True);
            Assert.That(map.CanAccess(pos2), Is.False);
            Assert.That(map.CanSeeThrough(pos2), Is.False);

            spatial.WorldPosition = pos1;

            Assert.That(map.CanAccess(pos1), Is.False);
            Assert.That(map.CanSeeThrough(pos1), Is.False);
            Assert.That(map.CanAccess(pos2), Is.True);
            Assert.That(map.CanSeeThrough(pos2), Is.True);
        }

        /// <summary>
        /// When an entity is deleted, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshMapTileFlagsOnEntityDeleted()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(IdDummySolidOpaque, map.AtPos(pos));

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            entMan.DeleteEntity(ent);

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);
        }

        /// <summary>
        /// When an entity's liveness changes, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshMapTileFlagsOnEntityLiveness()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMap(map.Id).MapEntityUid;

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(IdDummySolidOpaque, map.AtPos(pos));
            var metaData = entMan.GetComponent<MetaDataComponent>(ent);

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            metaData.Liveness = EntityGameLiveness.Hidden;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);

            metaData.Liveness = EntityGameLiveness.Alive;

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            metaData.Liveness = EntityGameLiveness.DeadAndBuried;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);
        }

        /// <summary>
        ///     Tests that changing solidity/opacity updates the corresponding map tile.
        /// </summary>
        [Test]
        public void TestMapSolidOpaqueRefresh()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(IdDummySolidOpaque, map.AtPos(pos));
            var entSpatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.That(map.CanSeeThrough(entSpatial.WorldPosition), Is.False);
            Assert.That(map.CanAccess(entSpatial.WorldPosition), Is.False);

            entSpatial.IsSolid = false;
            entSpatial.IsOpaque = false;

            Assert.That(map.CanSeeThrough(entSpatial.WorldPosition), Is.True);
            Assert.That(map.CanAccess(entSpatial.WorldPosition), Is.True);
        }

        [Test]
        public void TestMoveBetweenMapsRefresh()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            
            var map1 = sim.ActiveMap!;
            var map2 = sim.Resolve<IMapManager>().CreateMap(50, 50);

            var coords1 = new EntityCoordinates(map1.MapEntityUid, Vector2i.Zero);
            var coords2 = new EntityCoordinates(map2.MapEntityUid, Vector2i.Zero);

            var ent = entMan.SpawnEntity(IdDummySolidOpaque, coords1);

            var spatial = entMan.GetComponent<SpatialComponent>(ent);

            Assert.Multiple(() =>
            {
                Assert.That(map1.CanSeeThrough(coords1.Position), Is.False);
                Assert.That(map1.CanAccess(coords2.Position), Is.False);
                Assert.That(map2.CanSeeThrough(coords1.Position), Is.True);
                Assert.That(map2.CanAccess(coords2.Position), Is.True);
            });

            spatial.Coordinates = coords2;

            Assert.Multiple(() =>
            {
                Assert.That(map1.CanSeeThrough(coords1.Position), Is.True);
                Assert.That(map1.CanAccess(coords2.Position), Is.True);
                Assert.That(map2.CanSeeThrough(coords1.Position), Is.False);
                Assert.That(map2.CanAccess(coords2.Position), Is.False);
            });
        }

        private class Subscriber : IEntityEventSubscriber { }
    }
}

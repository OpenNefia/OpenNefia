using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture, Parallelizable]
    class MovementSystemTests
    {
        private static readonly string Prototypes = $@"
- type: Entity
  id: dummySolidOpaque
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
        /// When the local position of the spatial component changes, a PositionChangedEvent is raised.
        /// </summary>
        [Test]
        public void TestRaiseMoveEvent()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMapEntity(map.Id);

            var subscriber = new Subscriber();
            int calledCount = 0;
            entMan.EventBus.SubscribeEvent<EntPositionChangedEvent>(EventSource.Local, subscriber, MoveEventHandler);
            var ent1 = entMan.SpawnEntity(null, new MapCoordinates(Vector2i.Zero, map.Id));

            ent1.Spatial.WorldPosition = Vector2i.One;

            Assert.That(calledCount, Is.EqualTo(1));
            void MoveEventHandler(ref EntPositionChangedEvent ev)
            {
                calledCount++;
                Assert.That(ev.OldPosition, Is.EqualTo(new EntityCoordinates(mapEnt.Uid, Vector2i.Zero)));
                Assert.That(ev.NewPosition, Is.EqualTo(new EntityCoordinates(mapEnt.Uid, Vector2i.One)));
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
            var mapEnt = sim.Resolve<IMapManager>().GetMapEntity(map.Id);

            var pos = Vector2i.One;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);

            entMan.SpawnEntity(new("dummySolidOpaque"), map.AtPos(pos));

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
            var mapEnt = sim.Resolve<IMapManager>().GetMapEntity(map.Id);

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(new("dummySolidOpaque"), map.AtPos(pos));

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            ent.Spatial.WorldPosition += (1, 1);

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);

            ent.Spatial.WorldPosition = pos;

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);
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
            var mapEnt = sim.Resolve<IMapManager>().GetMapEntity(map.Id);

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(new("dummySolidOpaque"), map.AtPos(pos));

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            ent.Delete();

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);
        }

        /// <summary>
        /// When an entity is deleted, the accessibility of the corresponding map tile should be updated.
        /// </summary>
        [Test]
        public void TestRefreshMapTileFlagsOnEntityLiveness()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;
            var mapEnt = sim.Resolve<IMapManager>().GetMapEntity(map.Id);

            var pos = Vector2i.One;

            var ent = entMan.SpawnEntity(new("dummySolidOpaque"), map.AtPos(pos));

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            ent.MetaData.Liveness = EntityGameLiveness.Hidden;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);

            ent.MetaData.Liveness = EntityGameLiveness.Alive;

            Assert.That(map.CanAccess(pos), Is.False);
            Assert.That(map.CanSeeThrough(pos), Is.False);

            ent.MetaData.Liveness = EntityGameLiveness.DeadAndBuried;

            Assert.That(map.CanAccess(pos), Is.True);
            Assert.That(map.CanSeeThrough(pos), Is.True);
        }

        private class Subscriber : IEntityEventSubscriber { }
    }
}

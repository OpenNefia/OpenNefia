using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture, Parallelizable]
    class MovementSystemTests
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
        /// When the local position of the spatial component changes, a PositionChangedEvent is raised.
        /// </summary>
        [Test]
        public void OnMove_LocalPosChanged_RaiseMoveEvent()
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

        private class Subscriber : IEntityEventSubscriber { }
    }
}

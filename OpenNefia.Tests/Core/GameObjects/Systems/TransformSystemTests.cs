using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Tests.Core.GameObjects.Systems
{
    [TestFixture, Parallelizable]
    class TransformSystemTests
    {
        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .InitializeInstance();

            sim.SetActiveMap(new Map(50, 50));

            return sim;
        }

        /// <summary>
        /// When the local position of the transform changes, a MoveEvent is raised.
        /// </summary>
        [Test]
        public void OnMove_LocalPosChanged_RaiseMoveEvent()
        {
            var sim = SimulationFactory();
            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.ActiveMap!;

            var subscriber = new Subscriber();
            int calledCount = 0;
            entMan.EventBus.SubscribeEvent<OnMoveEventArgs>(EventSource.Local, subscriber, MoveEventHandler);
            var ent1 = entMan.SpawnEntity(null, map, Vector2i.Zero);

            ent1.Spatial.Pos = Vector2i.One;

            Assert.That(calledCount, Is.EqualTo(1));
            void MoveEventHandler(OnMoveEventArgs ev)
            {
                calledCount++;
                Assert.That(ev.OldPosition, Is.EqualTo(new MapCoordinates(map, Vector2i.Zero)));
                Assert.That(ev.NewPosition, Is.EqualTo(new MapCoordinates(map, Vector2i.One)));
            }
        }

        private class Subscriber : IEntityEventSubscriber { }
    }
}

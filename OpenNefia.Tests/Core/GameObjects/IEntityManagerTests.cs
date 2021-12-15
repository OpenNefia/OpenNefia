using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Tests.Core.GameObjects
{
    [TestFixture, Parallelizable]
    class EntityManagerTests
    {
        private static readonly MapId TestMapId = new(1);

        const string PROTOTYPE = @"
- type: Entity
  name: dummy
  id: dummy
  components:
  - type: Transform
";

        private static ISimulation SimulationFactory()
        {
            var sim = GameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(PROTOTYPE))
                .InitializeInstance();

            sim.CreateMapAndSetActive(50, 50);

            return sim;
        }

        /// <summary>
        /// The entity prototype can define field on the TransformComponent, just like any other component.
        /// </summary>
        [Test]
        public void SpawnEntity_PrototypeTransform_Works()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var map = sim.Resolve<IMapManager>().ActiveMap!;
            var newEnt = entMan.SpawnEntity(new("dummy"), map.AtPos(Vector2i.Zero));
            Assert.That(newEnt, Is.Not.Null);
            Assert.That(newEnt.Spatial.MapID, Is.EqualTo(map.Id));
            Assert.That(newEnt.Spatial.MapPosition.Position, Is.EqualTo(Vector2i.Zero));
        }
    }
}

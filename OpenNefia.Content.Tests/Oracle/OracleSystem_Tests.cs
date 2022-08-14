using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Oracle;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Oracle
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(OracleSystem))]
    public class OracleSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  parent: BaseItem
  components:
  - type: Quality
    quality: Unique
";

        [Test]
        public void TestOracleSystem()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var mapMan = sim.Resolve<IMapManager>();

            var oracle = sim.GetEntitySystem<IOracleSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            Assert.That(oracle.ArtifactLocations, Is.Empty);

            entGen.SpawnEntity(TestEntity, map);

            Assert.That(oracle.ArtifactLocations.Count, Is.EqualTo(1));
        }
    }
}
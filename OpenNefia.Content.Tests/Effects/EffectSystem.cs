using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Effects
{
    [TestFixture]
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestOf(typeof(NewEffectSystem))]
    public class EffectSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
";

        [Test]
        public void Test_Retargeting()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var sys = sim.GetEntitySystem<INewEffectSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            Assert.Multiple(() =>
            {
                Assert.That(false, Is.True);
            });
        }
    }
}
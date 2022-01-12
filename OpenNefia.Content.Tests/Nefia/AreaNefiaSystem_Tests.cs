using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.RandomAreas;

namespace OpenNefia.Content.Tests.Areas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(AreaNefiaSystem))]
    public class AreaNefiaSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestAreaEntityID = new("TestAreaEntity");

        private static readonly AreaFloorId TestAreaFloorID = new("Test.AreaFloor");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestAreaEntityID}
  parent: BaseArea
  components:
  - type: AreaNefia
";

        [Test]
        [TestCase(NefiaState.Unvisited, true)]
        [TestCase(NefiaState.Visited, true)]
        [TestCase(NefiaState.Conquered, false)]
        [TestCase(NefiaState.BossVanished, false)]
        public void TestNefiaSystem_RandomAreaActive(NefiaState state, bool isActive)
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var areaMan = sim.Resolve<IAreaManager>();
            var areaRandomGen = sim.GetEntitySystem<AreaRandomGenSystem>();

            var area = areaMan.CreateArea(TestAreaEntityID);

            var areaNefia = entMan.GetComponent<AreaNefiaComponent>(area.AreaEntityUid);
            areaNefia.State = state;

            Assert.That(areaRandomGen.IsRandomAreaActive(area), Is.EqualTo(isActive));
        }
    }
}

using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Inventory
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(InventorySystem))]
    public class InventorySystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
  - type: Inventory
    maxItemCount: 2
";

        [Test]
        public void TestIsInventoryFull()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var sys = sim.GetEntitySystem<IInventorySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var ent = entGen.SpawnEntity(TestEntity, map)!.Value;
            var inv = entMan.GetComponent<InventoryComponent>(ent);

            Assert.That(sys.IsInventoryFull(ent, inv), Is.False);

            entGen.SpawnEntity(TestEntity, inv.Container);
            Assert.That(sys.IsInventoryFull(ent, inv), Is.False);

            entGen.SpawnEntity(TestEntity, inv.Container);
            Assert.That(sys.IsInventoryFull(ent, inv), Is.True);
        }
    }
}
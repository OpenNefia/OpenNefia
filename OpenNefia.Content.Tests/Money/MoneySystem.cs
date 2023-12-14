using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Currency;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Money;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Money
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(MoneySystem))]
    public class MoneySystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");
        private static readonly PrototypeId<EntityPrototype> TestGoldPiece = new("TestGoldPiece");
        private static readonly PrototypeId<EntityPrototype> TestPlatinumCoin = new("TestPlatinumCoin");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  parent: BaseChara
  components:
  - type: Spatial
- type: Entity
  id: {TestGoldPiece}
  parent: BaseItem
  components:
  - type: GoldPiece
- type: Entity
  id: {TestPlatinumCoin}
  parent: BaseItem
  components:
  - type: PlatinumCoin
";

        [Test]
        public void TestMoneySystem()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();
            var stacks = sim.GetEntitySystem<IStackSystem>();

            var sys = sim.GetEntitySystem<IMoneySystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entGen.SpawnEntity(TestEntity, map.AtPos(0, 0));
            var inv = entMan.EnsureComponent<InventoryComponent>(ent!.Value).Container;

            var invGold = entGen.SpawnEntity(TestGoldPiece, inv, amount: 1000);
            var invPlatinum = entGen.SpawnEntity(TestPlatinumCoin, inv, amount: 50);

            var mapGold = entGen.SpawnEntity(TestGoldPiece, map.AtPos(0, 0), amount: 1000);
            var mapGoldRandom = entGen.SpawnEntity(TestGoldPiece, map.AtPos(0, 0));
            var mapPlatinum = entGen.SpawnEntity(TestPlatinumCoin, map.AtPos(0, 0), amount: 50);

            var money = entMan.GetComponent<MoneyComponent>(ent.Value);

            Assert.Multiple(() =>
            {
                Assert.That(entMan.IsAlive(invGold), Is.False, "Inv gold alive");
                Assert.That(entMan.IsAlive(invPlatinum), Is.False, "Inv platinum alive");
                Assert.That(entMan.IsAlive(mapGold), Is.True, "Map gold alive");
                Assert.That(entMan.IsAlive(mapPlatinum), Is.True, "Map platinum alive");

                Assert.That(money.Gold, Is.EqualTo(1099), "Inv gold amount");
                Assert.That(money.Platinum, Is.EqualTo(50), "Inv platinum amount");
                Assert.That(stacks.GetCount(mapGold), Is.EqualTo(1000), "Map gold amount");
                Assert.That(stacks.GetCount(mapGoldRandom), Is.GreaterThan(0), "Map gold random amount"); // Randomized
                Assert.That(stacks.GetCount(mapPlatinum), Is.EqualTo(50), "Map platinum amount");
            });
        }
    }
}
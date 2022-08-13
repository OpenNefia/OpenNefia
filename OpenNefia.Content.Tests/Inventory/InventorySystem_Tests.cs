using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
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
  - type: Skills
    skills:
      Elona.AttrSpeed: 100
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

        [Test]
        public void TestInventorySystem_SpeedPenalty()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var ent = entGen.SpawnEntity(TestEntity, map);
            var inv = entMan.GetComponent<InventoryComponent>(ent!.Value);

            // Inventory speed penalty only applies to the player.
            var gameSess = sim.Resolve<IGameSessionManager>();
            gameSess.Player = ent.Value;

            Assert.Multiple(() =>
            {
                var ev = new EntityRefreshSpeedEvent();
                inv.BurdenType = BurdenType.None;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeed, Is.EqualTo(200));
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(1f));

                ev = new EntityRefreshSpeedEvent();
                inv.BurdenType = BurdenType.Light;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeed, Is.EqualTo(200));
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(0.9f));

                ev = new EntityRefreshSpeedEvent();
                inv.BurdenType = BurdenType.Moderate;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeed, Is.EqualTo(200));
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(0.6f));

                ev = new EntityRefreshSpeedEvent();
                inv.BurdenType = BurdenType.Heavy;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeed, Is.EqualTo(200));
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(0.1f));

                ev = new EntityRefreshSpeedEvent();
                gameSess.Player = EntityUid.Invalid;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeed, Is.EqualTo(200));
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(1f));
            });
        }
    }
}
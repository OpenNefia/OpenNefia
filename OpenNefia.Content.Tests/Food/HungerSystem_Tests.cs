using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Hunger;
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

namespace OpenNefia.Content.Tests.Food
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(HungerSystem))]
    public class HungerSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestChara = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestChara}
  parent: BaseChara
  components:
  - type: Skills
    skills:
      Elona.AttrSpeed: 100
";

        [Test]
        public void TestHungerSystem_SpeedPenalty()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var map = sim.CreateMapAndSetActive(10, 10);
            var ent = entGen.SpawnEntity(TestChara, map);
            var hunger = entMan.GetComponent<HungerComponent>(ent!.Value);

            // Hunger speed penalty only applies to the player.
            var gameSess = sim.Resolve<IGameSessionManager>();
            gameSess.Player = ent.Value;

            Assert.Multiple(() =>
            {
                var ev = new EntityRefreshSpeedEvent();
                hunger.Nutrition = HungerLevels.Satisfied - 1;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(1f));

                ev = new EntityRefreshSpeedEvent();
                hunger.Nutrition = HungerLevels.Hungry - 1;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(0.9f));

                ev = new EntityRefreshSpeedEvent();
                hunger.Nutrition = HungerLevels.VeryHungry - 1;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(0.6f));

                ev = new EntityRefreshSpeedEvent();
                gameSess.Player = EntityUid.Invalid;
                entMan.EventBus.RaiseEvent(ent.Value, ref ev);
                Assert.That(ev.OutSpeedModifier, new ApproxEqualityConstraint(1f));
            });
        }
    }
}
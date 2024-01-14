using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;

namespace OpenNefia.Content.Tests.Buffs
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(VanillaBuffsSystem))]
    public class VanillaBuffsSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
  - type: Skills
    maxHP: 100
";

        [Test]
        public void Test_Contingency()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var buffs = sim.GetEntitySystem<IBuffSystem>();
            var damage = sim.GetEntitySystem<IDamageSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(TestEntity, map.AtPos(4, 4));

            Assert.Multiple(() =>
            {
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.Contingency, 100, 100), Is.True, "Add Contingency");
               
                damage.DamageHP(ent, 1000);
                Assert.That(entMan.IsAlive(ent), Is.True);

                buffs.HealAllBuffs(ent);
                damage.DamageHP(ent, 1000);
                Assert.That(entMan.IsAlive(ent), Is.False);
            });
        }
    }
}
using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Buffs;
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
    [TestFixture]
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestOf(typeof(BuffSystem))]
    public class BuffSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEntity = new("TestEntity");

        private static readonly string Prototypes = $@"
- type: Entity
  id: {TestEntity}
  components:
  - type: Spatial
  - type: Buffs
  - type: GrowthBuffs
  - type: Skills
";

        [Test]
        public void Test_HasBuff()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protos => protos.LoadString(Prototypes))
                .InitializeInstance();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();
            var entGen = sim.GetEntitySystem<IEntityGen>();

            var buffs = sim.GetEntitySystem<IBuffSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(TestEntity, map.AtPos(4, 4));

            Assert.Multiple(() =>
            {
                Assert.That(buffs.AddBuff(ent, Protos.Buff.ElementScar, 100, 100), Is.True, "Add buff");
                Assert.That(buffs.AddBuff(ent, Protos.Buff.ElementScar, 100, 100), Is.False, "Add duplicate buff");
                Assert.That(buffs.HasBuff(ent, Protos.Buff.ElementScar), Is.True, "HasBuff (prototype ID)");
                Assert.That(buffs.HasBuff<BuffElementScarComponent>(ent), Is.True, "HasBuff (component type)");
            });
        }
    }
}
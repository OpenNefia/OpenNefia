using NUnit.Framework;
using OpenNefia.Content.Areas;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.Resists;
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
  - type: Resists
  - type: Quality
  - type: Faction
    relationToPlayer: Enemy
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
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.ElementScar, 100, 100), Is.True, "Add buff");
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.ElementScar, 100, 100), Is.False, "Add duplicate buff");
                Assert.That(buffs.HasBuff(ent, Protos.Buff.ElementScar), Is.True, "HasBuff (prototype ID)");
                Assert.That(buffs.HasBuff<BuffElementScarComponent>(ent), Is.True, "HasBuff (component type)");
            });
        }

        [Test]
        public void Test_AddBuff_HolyVeilRepels()
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
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.MistOfFrailness, 100, 100), Is.True, "Add hex");
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.HolyVeil, 10000, 100), Is.True, "Add Holy Veil");
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.MistOfSilence, 100, 100), Is.False, "Add second hex");
            });
        }

        [Test]
        public void Test_AddBuff_MagicResistanceRepels()
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
            var resists = entMan.GetComponent<ResistsComponent>(ent);

            Assert.Multiple(() =>
            {
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.MistOfFrailness, 100, 100), Is.True, "Add hex");
                resists.Ensure(Protos.Element.Magic).Level.Base = 2000;
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.MistOfSilence, 100, 100), Is.False, "Add second hex");
            });
        }

        [Test]
        public void Test_AddBuff_QualityRepels()
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
            var quality = entMan.GetComponent<QualityComponent>(ent);

            Assert.Multiple(() =>
            {
                quality.Quality.Base = Quality.Great;
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.DeathWord, 100, 100), Is.False, "Add hex (blocked)");
                quality.Quality.Base = Quality.Bad;
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.DeathWord, 100, 100), Is.True, "Add hex");
            });
        }

        [Test]
        public void Test_AddBuff_QualityRepels_Ally()
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
            var quality = entMan.GetComponent<QualityComponent>(ent);
            var faction = entMan.GetComponent<FactionComponent>(ent);

            quality.Quality.Base = Quality.Great;
            faction.RelationToPlayer = Relation.Ally;

            Assert.That(buffs.TryAddBuff(ent, Protos.Buff.DeathWord, 100, 100), Is.True, "Add hex");
        }

        [Test]
        public void Test_HealAllBuffs()
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
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.ElementalShield, 100, 100), Is.True, "Add blessing");
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.ElementScar, 100, 100), Is.True, "Add hex");
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.Punishment, 100, 100), Is.True, "Add Punishment hex");

                buffs.HealAllBuffs(ent);

                Assert.That(buffs.HasBuff(ent, Protos.Buff.ElementalShield), Is.False, "Has blessing");
                Assert.That(buffs.HasBuff(ent, Protos.Buff.ElementScar), Is.False, "Has hex");
                Assert.That(buffs.HasBuff(ent, Protos.Buff.Punishment), Is.True, "Has Punishment");
            });
        }

        [Test]
        public void Test_AddBuff_AdjustPower()
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
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.Boost, 100, out var buff1), Is.True, "Add blessing (fixed power)");
                Assert.That(buff1!.BasePower, Is.EqualTo(100), "Base power");
                Assert.That(buff1.Power, Is.EqualTo(120), "Power");
            });
        }

        [Test]
        public void Test_AddBuff_AdjustDuration()
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
                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.DeathWord, 100, out var buff1), Is.True, "Add hex (fixed turns)");
                Assert.That(buff1!.TurnsRemaining, Is.EqualTo(20));

                buffs.HealAllBuffs(ent);

                Assert.That(buffs.TryAddBuff(ent, Protos.Buff.DeathWord, 100, out var buff2, 123), Is.True, "Add hex (fixed turns, overridden)");
                Assert.That(buff2!.TurnsRemaining, Is.EqualTo(123));
            });
        }
    }
}
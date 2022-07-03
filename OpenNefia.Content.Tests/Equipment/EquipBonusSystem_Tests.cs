using NUnit.Framework;
using OpenNefia.Content.Combat;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Tests.Equipment
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(EquipStatsSystem))]
    public class EquipBonusSystem_Tests : OpenNefiaUnitTest
    {
        private static readonly PrototypeId<EntityPrototype> TestEquipmentID = new("TestEquipment");
        private static readonly PrototypeId<EntityPrototype> TestCharaID = new("TestChara");

        private static readonly string Prototypes = @$"
- type: Entity
  id: {TestEquipmentID}
  components:
  - type: Equipment
    equipSlots:
    - {EquipSlot.Hand}

- type: Entity
  id: {TestCharaID}
  parent: BaseChara
  components:
  - type: Chara
    race: Elona.Slug
    class: Elona.Predator
";

        private ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .InitializeInstance();

            return sim;
        }

        [Test]
        public void TestApplyEquipBonus()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();
            var refreshSys = sim.GetEntitySystem<IRefreshSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var entChara = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;
            var entEquipment = entGen.SpawnEntity(TestEquipmentID, map.AtPos(Vector2i.One))!.Value;

            var equipperStats = entMan.EnsureComponent<EquipStatsComponent>(entChara);

            equipperStats.DV.Base = 10;
            equipperStats.PV.Base = 11;
            equipperStats.HitBonus.Base = 12;
            equipperStats.DamageBonus.Base = 13;
            equipperStats.PierceRate.Base = 14;
            equipperStats.CriticalRate.Base = 15;

            var equipStats = entMan.EnsureComponent<EquipStatsComponent>(entEquipment);

            equipStats.DV.Base = 10;
            equipStats.PV.Base = 11;
            equipStats.HitBonus.Base = 12;
            equipStats.DamageBonus.Base = 13;
            equipStats.PierceRate.Base = 14;
            equipStats.CriticalRate.Base = 15;

            refreshSys.Refresh(entChara);
            refreshSys.Refresh(entEquipment);

            Assert.Multiple(() =>
            {
                Assert.That(equipperStats.DV.Buffed, Is.EqualTo(10));
                Assert.That(equipperStats.PV.Buffed, Is.EqualTo(11));
                Assert.That(equipperStats.HitBonus.Buffed, Is.EqualTo(12));
                Assert.That(equipperStats.DamageBonus.Buffed, Is.EqualTo(13));
                Assert.That(equipperStats.PierceRate.Buffed, Is.EqualTo(14));
                Assert.That(equipperStats.CriticalRate.Buffed, Is.EqualTo(15));
            });

            Assert.That(equipSlotSys.TryGetEmptyEquipSlot(entChara, EquipSlot.Hand, out var equipSlot), Is.True);
            Assert.That(equipSlotSys.TryEquip(entChara, entEquipment, equipSlot!), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(equipperStats.DV.Buffed, Is.EqualTo(20));
                Assert.That(equipperStats.PV.Buffed, Is.EqualTo(22));
                Assert.That(equipperStats.HitBonus.Buffed, Is.EqualTo(24));
                Assert.That(equipperStats.DamageBonus.Buffed, Is.EqualTo(26));
                Assert.That(equipperStats.PierceRate.Buffed, Is.EqualTo(28));
                Assert.That(equipperStats.CriticalRate.Buffed, Is.EqualTo(30));

                Assert.That(equipSlotSys.TryUnequip(entChara, equipSlot!, out _), Is.True);

                Assert.That(equipperStats.DV.Buffed, Is.EqualTo(10));
                Assert.That(equipperStats.PV.Buffed, Is.EqualTo(11));
                Assert.That(equipperStats.HitBonus.Buffed, Is.EqualTo(12));
                Assert.That(equipperStats.DamageBonus.Buffed, Is.EqualTo(13));
                Assert.That(equipperStats.PierceRate.Buffed, Is.EqualTo(14));
                Assert.That(equipperStats.CriticalRate.Buffed, Is.EqualTo(15));
            });
        }
    }
}

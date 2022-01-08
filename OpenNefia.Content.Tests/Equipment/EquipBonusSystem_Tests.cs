using NUnit.Framework;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
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
    [TestFixture]
    [TestOf(typeof(EquipBonusSystem))]
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

            var skills = entMan.EnsureComponent<SkillsComponent>(entChara);

            skills.DV.Base = 10;
            skills.PV.Base = 11;
            skills.HitBonus.Base = 12;
            skills.DamageBonus.Base = 13;

            var equipBonus = entMan.EnsureComponent<EquipBonusComponent>(entEquipment);

            equipBonus.DV.Base = 10;
            equipBonus.PV.Base = 11;
            equipBonus.HitBonus.Base = 12;
            equipBonus.DamageBonus.Base = 13;

            refreshSys.Refresh(entChara);
            refreshSys.Refresh(entEquipment);

            Assert.Multiple(() =>
            {
                Assert.That(skills.DV.Buffed, Is.EqualTo(10));
                Assert.That(skills.PV.Buffed, Is.EqualTo(11));
                Assert.That(skills.HitBonus.Buffed, Is.EqualTo(12));
                Assert.That(skills.DamageBonus.Buffed, Is.EqualTo(13));
            });

            Assert.That(equipSlotSys.TryGetEmptyEquipSlot(entChara, EquipSlot.Hand, out var equipSlot), Is.True);
            Assert.That(equipSlotSys.TryEquip(entChara, entEquipment, equipSlot!), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(skills.DV.Buffed, Is.EqualTo(20));
                Assert.That(skills.PV.Buffed, Is.EqualTo(22));
                Assert.That(skills.HitBonus.Buffed, Is.EqualTo(24));
                Assert.That(skills.DamageBonus.Buffed, Is.EqualTo(26));

                Assert.That(equipSlotSys.TryUnequip(entChara, equipSlot!, out _), Is.True);

                Assert.That(skills.DV.Buffed, Is.EqualTo(10));
                Assert.That(skills.PV.Buffed, Is.EqualTo(11));
                Assert.That(skills.HitBonus.Buffed, Is.EqualTo(12));
                Assert.That(skills.DamageBonus.Buffed, Is.EqualTo(13));
            });
        }
    }
}

using NUnit.Framework;
using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.RandomGen;
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

namespace OpenNefia.Content.Tests.Charas
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(CharaSystem))]
    public class CharaSystem_Tests
    {
        private static readonly PrototypeId<EntityPrototype> TestCharaID = new("TestChara");

        private static readonly PrototypeId<EquipSlotPrototype> TestSlot1ID = new("TestSlot1");
        private static readonly PrototypeId<EquipSlotPrototype> TestSlot2ID = new("TestSlot2");

        private static readonly string Prototypes = @$"
- type: Elona.EquipSlot
  id: {TestSlot1ID}
- type: Elona.EquipSlot
  id: {TestSlot2ID}

- type: Elona.Race
  id: TestRace
  initialEquipSlots:
  - {TestSlot1ID}
  - {TestSlot2ID}
  - {TestSlot1ID}
  chipMale: {Chip.CharaRaceSlime}
  chipFemale: {Chip.CharaRaceSlime}
  components:
  - type: TestRace

- type: Elona.Class
  id: TestClass
  components:
  - type: TestClass

- type: Entity
  id: TestChara
  parent: BaseChara
  components:
  - type: Chara
    race: TestRace
    class: TestClass
";

        private ISimulation SimulationFactory()
        {
            var sim = ContentFullGameSimulation
                .NewSimulation()
                .RegisterPrototypes(protoMan => protoMan.LoadString(Prototypes))
                .RegisterComponents(factory =>
                {
                    factory.RegisterClass<TestClassComponent>();
                    factory.RegisterClass<TestRaceComponent>();
                })
                .InitializeInstance();

            return sim;
        }

        [Test]
        public void TestRaceEquipSlotsInitialization()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.HasEquipSlot(ent, TestSlot1ID), Is.True, "Has equip slot 1");
                Assert.That(equipSlotSys.HasEquipSlot(ent, TestSlot2ID), Is.True, "Has equip slot 2");
                Assert.That(equipSlotSys.HasEquipSlot(ent, EquipSlot.Ranged), Is.True, "Has ranged slot");
                Assert.That(equipSlotSys.HasEquipSlot(ent, EquipSlot.Ammo), Is.True, "Has ammo slot");

                Assert.That(entMan.TryGetComponent(ent, out EquipSlotsComponent equipSlots), Is.True, "Has inventory component");

                // 3 slots for the race, plus 2 more for ranged/ammo.
                Assert.That(equipSlots.EquipSlots.Count, Is.EqualTo(5), "Equip slots count");

                Assert.That(equipSlots.EquipSlots[0].ID, Is.EqualTo(TestSlot1ID), "Equip slot 1 ID");
                Assert.That(equipSlots.EquipSlots[1].ID, Is.EqualTo(TestSlot2ID), "Equip slot 2 ID");
                Assert.That(equipSlots.EquipSlots[2].ID, Is.EqualTo(TestSlot1ID), "Equip slot 3 ID");
                Assert.That(equipSlots.EquipSlots[3].ID, Is.EqualTo(EquipSlot.Ranged), "Equip slot 4 ID (ranged)");
                Assert.That(equipSlots.EquipSlots[4].ID, Is.EqualTo(EquipSlot.Ammo), "Equip slot 5 ID (ammo)");
            });
        }

        [Test]
        public void TestClassAndRaceComponentsInitialization()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var entGen = sim.GetEntitySystem<IEntityGen>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entGen.SpawnEntity(TestCharaID, map.AtPos(Vector2i.One))!.Value;

            Assert.Multiple(() =>
            {
                Assert.That(entMan.GetComponent<CharaComponent>(ent).RaceSlot, Is.Not.Null, "Has race slot");
                Assert.That(entMan.GetComponent<CharaComponent>(ent).ClassSlot, Is.Not.Null, "Has class slot");
                Assert.That(entMan.HasComponent<TestRaceComponent>(ent), Is.True, "Has race component");
                Assert.That(entMan.HasComponent<TestClassComponent>(ent), Is.True, "Has class component");
            });
        }

        /// <summary>
        /// Values are from a mocked version of 1.22.
        /// </summary>
        [Test]
        public void TestLevelOverrideSkillScaling()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var charaGen = sim.GetEntitySystem<ICharaGen>();
            var skills = sim.GetEntitySystem<ISkillsSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var args = new EntityGenCommonArgs()
            {
                NoRandomModify = true
            };
            var ent = charaGen.GenerateChara(map.AtPos(Vector2i.One), Protos.Chara.Putit, args: EntityGenArgSet.Make(args));
            Assert.That(entMan.IsAlive(ent), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(skills.BaseLevel(ent!.Value, Protos.Skill.AttrStrength), Is.EqualTo(4), "Strength");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrConstitution), Is.EqualTo(5), "Constitution");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrDexterity), Is.EqualTo(7), "Dexterity");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLearning), Is.EqualTo(6), "Learning");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrWill), Is.EqualTo(8), "Will");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrPerception), Is.EqualTo(5), "Perception");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrMagic), Is.EqualTo(4), "Magic");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrCharisma), Is.EqualTo(14), "Charisma");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrSpeed), Is.EqualTo(56), "Speed");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLuck), Is.EqualTo(50), "Luck");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLife), Is.EqualTo(80), "Life");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrMana), Is.EqualTo(100), "Mana");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxHP, Is.EqualTo(9), "Max HP");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxMP, Is.EqualTo(4), "Max MP");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxStamina, Is.EqualTo(102), "Max Stamina");

                Assert.That(skills.Potential(ent!.Value, Protos.Skill.AttrStrength), Is.EqualTo(81), "Strength Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrConstitution), Is.EqualTo(101), "Constitution Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrDexterity), Is.EqualTo(141), "Dexterity Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLearning), Is.EqualTo(121), "Learning Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrWill), Is.EqualTo(161), "Will Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrPerception), Is.EqualTo(101), "Perception Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrMagic), Is.EqualTo(81), "Magic Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrCharisma), Is.EqualTo(261), "Charisma Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrSpeed), Is.EqualTo(400), "Speed Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLuck), Is.EqualTo(100), "Luck Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLife), Is.EqualTo(100), "Life Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrMana), Is.EqualTo(100), "Mana Potential");
            });

            args.LevelOverride = 100;
            ent = charaGen.GenerateChara(map.AtPos(Vector2i.One), Protos.Chara.Putit, args: EntityGenArgSet.Make(args));
            Assert.That(entMan.IsAlive(ent), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(skills.BaseLevel(ent!.Value, Protos.Skill.AttrStrength), Is.EqualTo(84), "Strength");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrConstitution), Is.EqualTo(93), "Constitution");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrDexterity), Is.EqualTo(116), "Dexterity");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLearning), Is.EqualTo(104), "Learning");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrWill), Is.EqualTo(130), "Will");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrPerception), Is.EqualTo(93), "Perception");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrMagic), Is.EqualTo(84), "Magic");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrCharisma), Is.EqualTo(229), "Charisma");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrSpeed), Is.EqualTo(165), "Speed");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLuck), Is.EqualTo(50), "Luck");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrLife), Is.EqualTo(80), "Life");
                Assert.That(skills.BaseLevel(ent.Value, Protos.Skill.AttrMana), Is.EqualTo(100), "Mana");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxHP, Is.EqualTo(1081), "Max HP");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxMP, Is.EqualTo(1412), "Max MP");
                Assert.That(entMan.GetComponent<SkillsComponent>(ent.Value).MaxStamina, Is.EqualTo(144), "Max Stamina");

                Assert.That(skills.Potential(ent!.Value, Protos.Skill.AttrStrength), Is.EqualTo(2), "Strength Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrConstitution), Is.EqualTo(2), "Constitution Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrDexterity), Is.EqualTo(2), "Dexterity Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLearning), Is.EqualTo(2), "Learning Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrWill), Is.EqualTo(2), "Will Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrPerception), Is.EqualTo(2), "Perception Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrMagic), Is.EqualTo(2), "Magic Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrCharisma), Is.EqualTo(2), "Charisma Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrSpeed), Is.EqualTo(2), "Speed Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLuck), Is.EqualTo(100), "Luck Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrLife), Is.EqualTo(100), "Life Potential");
                Assert.That(skills.Potential(ent.Value, Protos.Skill.AttrMana), Is.EqualTo(100), "Mana Potential");
            });
        }

        private class TestRaceComponent : Component
        {
        }

        private class TestClassComponent : Component
        {
        }
    }
}

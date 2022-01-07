using NUnit.Framework;
using OpenNefia.Content.EquipSlots;
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

namespace OpenNefia.Content.Tests.EquipSlots
{
    [TestFixture]
    [TestOf(typeof(EquipSlotsSystem))]
    public class EquipSlotsSystem_Tests
    {
        private static readonly PrototypeId<EquipSlotPrototype> TestSlot1ID = new("TestSlot1");
        private static readonly PrototypeId<EquipSlotPrototype> TestSlot2ID = new("TestSlot2");
        private static readonly PrototypeId<EquipSlotPrototype> InvalidID = new("Invalid");

        private static readonly PrototypeId<EntityPrototype> TestEquipment1ID = new("TestEquipment1");

        private static readonly string Prototypes = @$"
- type: Elona.EquipSlot
  id: {TestSlot1ID}
- type: Elona.EquipSlot
  id: {TestSlot2ID}

- type: Entity
  id: {TestEquipment1ID}
  components:
  - type: Equipment
    validEquipSlots:
    - {TestSlot1ID}
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
        public void TestInitializeEquipSlots()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            Assert.That(entMan.HasComponent<EquipSlotsComponent>(ent), Is.False);
            Assert.That(entMan.HasComponent<ContainerManagerComponent>(ent), Is.False);

            List<PrototypeId<EquipSlotPrototype>> equipSlotProtos = new() 
            { 
                TestSlot1ID,
                TestSlot2ID,
                InvalidID,
                TestSlot2ID,
            };

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(entMan.HasComponent<EquipSlotsComponent>(ent), Is.True);
                Assert.That(entMan.HasComponent<ContainerManagerComponent>(ent), Is.True);

                Assert.That(equipSlotSys.TryGetEquipSlots(ent, out var equipSlots), Is.True);
                Assert.That(equipSlots!.Count, Is.EqualTo(3));

                Assert.That(equipSlots[0].ID, Is.EqualTo(TestSlot1ID));
                Assert.That(equipSlots[1].ID, Is.EqualTo(TestSlot2ID));
                Assert.That(equipSlots[2].ID, Is.EqualTo(TestSlot2ID));

                Assert.That((string)equipSlots[0].ContainerID, Is.EqualTo($"Elona.EquipSlot:TestSlot1:0"));
                Assert.That((string)equipSlots[1].ContainerID, Is.EqualTo($"Elona.EquipSlot:TestSlot2:0"));
                Assert.That((string)equipSlots[2].ContainerID, Is.EqualTo($"Elona.EquipSlot:TestSlot2:1"));

                Assert.That(equipSlotSys.TryGetEquipSlotAndContainer(ent, TestSlot1ID, out var equipSlot1, out var containerSlot1), Is.True);
                Assert.That(equipSlotSys.TryGetEquipSlotAndContainer(ent, TestSlot2ID, out var equipSlot2, out var containerSlot2), Is.True);
                Assert.That(equipSlot1, Is.EqualTo(equipSlots[0]));
                Assert.That(equipSlot2, Is.EqualTo(equipSlots[1]));
                Assert.That(containerSlot1!.ID, Is.EqualTo(equipSlot1!.ContainerID));
                Assert.That(containerSlot2!.ID, Is.EqualTo(equipSlot2!.ContainerID));
            });
        }

        [Test]
        public void TestAddEquipSlots()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var containerSys = sim.GetEntitySystem<ContainerSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

             var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            Assert.That(entMan.HasComponent<EquipSlotsComponent>(ent), Is.False);
            Assert.That(entMan.HasComponent<ContainerManagerComponent>(ent), Is.False);
            var equipSlots = entMan.EnsureComponent<EquipSlotsComponent>(ent);
            var containers = entMan.EnsureComponent<ContainerManagerComponent>(ent);

            Assert.Multiple(() =>
            {
                var oldContainerCount = containers.Containers.Count;

                Assert.That(equipSlotSys.TryAddEquipSlot(ent, InvalidID, out _, out _), Is.False);

                Assert.That(equipSlotSys.TryAddEquipSlot(ent, TestSlot1ID, out var containerSlot, out var equipSlot), Is.True);
                Assert.That(equipSlotSys.HasEquipSlot(ent, TestSlot1ID), Is.True);

                Assert.That(equipSlots.EquipSlots!.Count, Is.EqualTo(1), "Equip slot count");
                Assert.That(containers.Containers.Count, Is.EqualTo(oldContainerCount + 1), "Container count");

                Assert.That(equipSlot!.ID, Is.EqualTo(TestSlot1ID));
                Assert.That((string)equipSlot.ContainerID, Is.EqualTo($"Elona.EquipSlot:TestSlot1:0"));
                Assert.That(containerSys.HasContainer(ent, equipSlot.ContainerID), Is.True);
            });
        }

        [Test]
        public void TestRemoveEquipSlots()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var containerSys = sim.GetEntitySystem<ContainerSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            var entItem = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));

            Assert.That(entMan.HasComponent<EquipSlotsComponent>(ent), Is.False);
            Assert.That(entMan.HasComponent<ContainerManagerComponent>(ent), Is.False);

            List<PrototypeId<EquipSlotPrototype>> equipSlotProtos = new()
            {
                TestSlot1ID,
                TestSlot2ID,
                TestSlot2ID,
            };

            var equipSlots = entMan.EnsureComponent<EquipSlotsComponent>(ent);
            var containers = entMan.EnsureComponent<ContainerManagerComponent>(ent);

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            var equipSlot = equipSlots.EquipSlots[0];
            Assert.That(equipSlotSys.TryGetContainerForEquipSlot(ent, equipSlot, out var container), Is.True);

            container!.Insert(entItem);

            Assert.Multiple(() =>
            {
                var oldContainerCount = containers.Containers.Count;

                Assert.That(equipSlotSys.TryRemoveEquipSlot(ent, equipSlot), Is.True);
                Assert.That(equipSlotSys.HasEquipSlot(ent, TestSlot1ID), Is.False);

                // Equipment in the slot should be deleted.
                Assert.That(entMan.EntityExists(entItem), Is.False);

                Assert.That(equipSlots.EquipSlots!.Count, Is.EqualTo(2), "Equip slot count");
                Assert.That(containers.Containers.Count, Is.EqualTo(oldContainerCount - 1), "Container count");

                Assert.That(equipSlotSys.TryRemoveEquipSlot(ent, equipSlot), Is.False);
            });
        }

        [Test]
        public void TestTryEquip()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var containerSys = sim.GetEntitySystem<ContainerSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            var entItem1 = entMan.SpawnEntity(TestEquipment1ID, map.AtPos(Vector2i.One));

            List<PrototypeId<EquipSlotPrototype>> equipSlotProtos = new()
            {
                TestSlot1ID,
            };

            var equipSlots = entMan.EnsureComponent<EquipSlotsComponent>(ent);
            var containers = entMan.EnsureComponent<ContainerManagerComponent>(ent);

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.CanEquip(ent, entItem1, TestSlot1ID, out _), Is.True, "Can equip 1");
                Assert.That(equipSlotSys.CanEquip(ent, entItem1, TestSlot2ID, out var reason), Is.False, "Can equip 2");
                Assert.That(reason, Is.EqualTo("Elona.EquipSlots.CannotEquip"));

                Assert.That(equipSlotSys.TryEquip(ent, entItem1, TestSlot2ID), Is.False, "Try equip 2");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, TestSlot1ID), Is.True, "Try equip 1");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, TestSlot1ID), Is.False, "Try equip 1 twice");

                Assert.That(equipSlotSys.TryGetSlotEntity(ent, TestSlot1ID, out var entEquipped), Is.True, "Try get slot entity");
                Assert.That(entEquipped, Is.EqualTo(entItem1), "Slot entity equality");
            });
        }

        [Test]
        public void TestTryUnequip()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var containerSys = sim.GetEntitySystem<ContainerSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            var entItem1 = entMan.SpawnEntity(TestEquipment1ID, map.AtPos(Vector2i.One));

            List<PrototypeId<EquipSlotPrototype>> equipSlotProtos = new()
            {
                TestSlot1ID,
            };

            var equipSlots = entMan.EnsureComponent<EquipSlotsComponent>(ent);
            var containers = entMan.EnsureComponent<ContainerManagerComponent>(ent);

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.CanUnequip(ent, TestSlot1ID, out var reason), Is.False, "Can unequip 1");
                Assert.That(reason, Is.EqualTo("Elona.EquipSlots.CannotUnequip"));
                Assert.That(equipSlotSys.CanUnequip(ent, TestSlot2ID, out reason), Is.False, "Can unequip 2");
                Assert.That(reason, Is.EqualTo("Elona.EquipSlots.CannotUnequip"));

                Assert.That(equipSlotSys.TryEquip(ent, entItem1, TestSlot1ID), Is.True);

                Assert.That(equipSlotSys.CanUnequip(ent, TestSlot1ID, out _), Is.True, "Can unequip 1 after equip");

                Assert.That(equipSlotSys.TryUnequip(ent, TestSlot2ID), Is.False, "Try unequip 2");
                Assert.That(equipSlotSys.TryUnequip(ent, TestSlot1ID), Is.True, "Try unequip 1");
                Assert.That(equipSlotSys.TryUnequip(ent, TestSlot1ID), Is.False, "Try unequip 1 twice");

                Assert.That(equipSlotSys.TryGetSlotEntity(ent, TestSlot1ID, out _), Is.False, "Try get slot entity");
            });
        }
    }
}

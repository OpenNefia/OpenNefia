using NUnit.Framework;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Inventory;
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
    equipSlots:
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

                Assert.That(equipSlotSys.TryAddEquipSlot(ent, TestSlot1ID, out var equipSlot, out var containerSlot), Is.True);
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
                TestSlot2ID,
            };

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot1), Is.True);
                Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot2ID, out var equipSlot2), Is.True);

                Assert.That(equipSlotSys.CanEquip(ent, entItem1, equipSlot1!, out _), Is.True, "Can equip 1");
                Assert.That(equipSlotSys.CanEquip(ent, entItem1, equipSlot2!, out var reason), Is.False, "Can equip 2");
                Assert.That(reason, Is.EqualTo("Elona.EquipSlots.Equip.Fails"));

                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.True, "Has empty equip slot");

                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot2!), Is.False, "Try equip 2");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot1!), Is.True, "Try equip 1");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot1!), Is.False, "Try equip 1 twice");

                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.False, "Has empty equip slot");

                Assert.That(equipSlotSys.TryGetSlotEntity(ent, equipSlot1!, out var entEquipped), Is.True, "Try get slot entity");
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

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot), Is.True, "Try get equip slot");
                Assert.That(equipSlotSys.CanUnequip(ent, equipSlot!, out var reason), Is.False, "Can unequip 1");
                Assert.That(reason, Is.EqualTo("Elona.EquipSlots.Unequip.Fails"));

                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot!), Is.True);

                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.False, "Has empty equip slot");

                Assert.That(equipSlotSys.CanUnequip(ent, equipSlot!, out _), Is.True, "Can unequip 1 after equip");

                Assert.That(equipSlotSys.TryUnequip(ent, equipSlot!), Is.True, "Try unequip 1");
                Assert.That(equipSlotSys.TryUnequip(ent, equipSlot!), Is.False, "Try unequip 1 twice");

                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.True, "Has empty equip slot");

                Assert.That(equipSlotSys.TryGetSlotEntity(ent, equipSlot!, out _), Is.False, "Try get slot entity");
            });
        }

        /// <summary>
        /// Test some edge case behavior relating to equipped entities that are deleted/become dead.
        /// </summary>
        [Test]
        public void TestTryGetEmptyEquipSlot()
        {
            var sim = SimulationFactory();

            var entMan = sim.Resolve<IEntityManager>();
            var mapMan = sim.Resolve<IMapManager>();

            var containerSys = sim.GetEntitySystem<ContainerSystem>();
            var equipSlotSys = sim.GetEntitySystem<EquipSlotsSystem>();

            var map = sim.CreateMapAndSetActive(10, 10);

            var ent = entMan.SpawnEntity(null, map.AtPos(Vector2i.One));
            var entItem1 = entMan.SpawnEntity(TestEquipment1ID, map.AtPos(Vector2i.One));

            var itemMetaData = entMan.GetComponent<MetaDataComponent>(entItem1);

            List<PrototypeId<EquipSlotPrototype>> equipSlotProtos = new()
            {
                TestSlot1ID,
            };

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot), Is.True, "Try get equip slot");
            Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot!), Is.True, "Try equip");
            Assert.That(equipSlotSys.TryGetContainerForEquipSlot(ent, equipSlot!, out var containerSlot), Is.True, "Try get container");

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.False, "Has empty equip slot");

                // Check entity liveness state change.
                itemMetaData.Liveness = EntityGameLiveness.DeadAndBuried;
                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.False, "Has empty equip slot (entity liveness)");

                // Check entity deletion.
                entMan.DeleteEntity(entItem1);
                Assert.That(equipSlotSys.HasEmptyEquipSlot(ent, TestSlot1ID), Is.True, "Has empty equip slot (entity deletion)");
            });
        }

        /// <summary>
        /// Tests that an unequipped item is moved to the specified container.
        /// </summary>
        [Test]
        public void TestTryUnequipPlacement_WithContainer()
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

            // If the enti, unequipping the item will make it be placed there.
            var inventory = entMan.EnsureComponent<InventoryComponent>(ent);

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot), Is.True, "Try get equip slot");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot!), Is.True, "Try equip 1");
                Assert.That(equipSlotSys.TryUnequip(ent, equipSlot!, placeInto: inventory.Container), Is.True, "Try unequip 1");

                var entItem1Spatial = entMan.GetComponent<SpatialComponent>(entItem1);
                Assert.That(entItem1Spatial.Coordinates, Is.EqualTo(new EntityCoordinates(ent, Vector2i.Zero)));
                Assert.That(inventory.Container.ContainedEntities.Contains(entItem1), Is.True);
            });
        }

        /// <summary>
        /// Tests that an unequipped item is moved to the parent map if no container is specified.
        /// </summary>
        [Test]
        public void TestTryUnequipPlacement_NoContainer()
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

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot), Is.True, "Try get equip slot");
                Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot!), Is.True, "Try equip 1");
                Assert.That(equipSlotSys.TryUnequip(ent, equipSlot!), Is.True, "Try unequip 1");

                var entItem1Spatial = entMan.GetComponent<SpatialComponent>(entItem1);
                Assert.That(entItem1Spatial.Coordinates, Is.EqualTo(map.AtPosEntity(Vector2i.One)), "New coordinates");
            });
        }

        [Test]
        public void TestTryGetContainerEnumerator()
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
                TestSlot1ID,
                TestSlot2ID,
            };

            equipSlotSys.InitializeEquipSlots(ent, equipSlotProtos);

            Assert.That(equipSlotSys.TryGetEmptyEquipSlot(ent, TestSlot1ID, out var equipSlot), Is.True, "Try get equip slot");
            Assert.That(equipSlotSys.TryEquip(ent, entItem1, equipSlot!), Is.True);

            Assert.Multiple(() =>
            {
                Assert.That(equipSlotSys.TryGetContainerSlotEnumerator(ent, out var enumerator), Is.True, "Try get enumerator");
                Assert.That(enumerator.MoveNext(out var containerSlot), Is.True, "MoveNext 1");
                Assert.That(containerSlot?.ContainedEntity.HasValue, Is.True, "HasValue 1");
                Assert.That(containerSlot!.ContainedEntity!.Value, Is.EqualTo(entItem1), "Value 1");
                Assert.That(enumerator.MoveNext(out containerSlot), Is.True, "MoveNext 2");
                Assert.That(containerSlot!.ContainedEntity.HasValue, Is.False, "HasValue 2");
                Assert.That(enumerator.MoveNext(out containerSlot), Is.True, "MoveNext 3");
                Assert.That(containerSlot!.ContainedEntity.HasValue, Is.False, "HasValue 3");
                Assert.That(enumerator.MoveNext(out _), Is.False, "MoveNext 4");
            });
        }
    }
}

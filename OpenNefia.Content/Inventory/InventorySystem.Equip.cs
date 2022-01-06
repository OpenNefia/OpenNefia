using OpenNefia.Content.Equipment;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Equipment.EquipSlotPrototype>;

namespace OpenNefia.Content.Inventory
{
    public sealed partial class InventorySystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ContainerSystem _containerSys = default!;

        // That's one beefy mutant.
        private const int MaxContainerSlotsPerEquipSlot = 1024;

        /// <summary>
        /// Clears and initializes equipment slots on this entity, adding an <see cref="InventoryComponent"/>
        /// and a <see cref="ContainerManagerComponent"/> if needed.
        /// </summary>
        /// <param name="uid">Entity UID.</param>
        /// <param name="equipSlotProtoIds">Equip slots for this entity.</param>
        public void InitializeEquipSlots(EntityUid uid, IEnumerable<EquipSlotPrototypeId> equipSlotProtoIds,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref inventory, logMissing: false))
                inventory = EntityManager.AddComponent<InventoryComponent>(uid);
            if (!Resolve(uid, ref containerComp, logMissing: false))
                containerComp = EntityManager.AddComponent<ContainerManagerComponent>(uid);

            ClearEquipSlots(uid, inventory, containerComp);
            
            foreach (var slotId in equipSlotProtoIds)
            {
                TryAddEquipSlot(uid, slotId, out _, out _, inventory, containerComp);
            }
        }

        public bool TryAddEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId,
            [NotNullWhen(true)] out ContainerSlot? containerSlot, [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            equipSlotInstance = null;
            if (!Resolve(uid, ref inventory, ref containerComp))
                return false;

            var containerId = FindFreeContainerIdForSlot(uid, slotId, inventory, containerComp);
            if (containerId == null)
                return false;

            equipSlotInstance = new EquipSlotInstance(slotId, containerId.Value);
            inventory.EquipSlots.Add(equipSlotInstance);

            containerSlot = _containerSys.MakeContainer<ContainerSlot>(uid, equipSlotInstance.ContainerID, containerManager: containerComp);
            containerSlot.OccludesLight = false;

            return true;
        }

        public bool TryRemoveEquipSlot(EntityUid uid, EquipSlotInstance instance,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref inventory, ref containerComp))
                return false;

            if (!inventory.EquipSlots.Contains(instance))
            {
                Logger.WarningS("inv.equip", $"Tried to remove equip slot {instance}, but it wasn't owned by entity {uid}!");
                return false;
            }
            
            if (_containerSys.TryGetContainer(uid, instance.ContainerID, out var container))
            {
                // Wipe the equipment item in this slot.
                _containerSys.CleanContainer(container);
                container.Shutdown();
            }
            else
            {
                Logger.WarningS("inv.equip", $"Tried to remove equip slot {instance} from entity {uid}, but its container was not found.");
            }

            inventory.EquipSlots.Remove(instance);
            return true;
        }

        public void ClearEquipSlots(EntityUid uid,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref inventory, ref containerComp))
                return;

            foreach (var slot in GetEquipSlots(uid))
            {
                TryRemoveEquipSlot(uid, slot, inventory, containerComp);
            }

            // In case any errors occurred.
            inventory.EquipSlots.Clear();
        }

        private static ContainerId MakeContainerId(EquipSlotPrototypeId slotId, int index)
        {
            return new($"Elona.EquipSlot:{slotId}:{index}");
        }

        private ContainerId? FindFreeContainerIdForSlot(EntityUid uid, EquipSlotPrototypeId slotId, 
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref inventory, ref containerComp))
                return null;

            var index = 0;

            while (index < MaxContainerSlotsPerEquipSlot)
            {
                var containerId = MakeContainerId(slotId, index);
                
                if (!_containerSys.HasContainer(uid, containerId, containerComp))
                {
                    return containerId;
                }
                
                index++;
            }

            Logger.WarningS("inv.equip", $"Could not find free container ID for equipment slot {slotId} (entity {uid}) after {index} tries!");

            return null;
        }
        
        /// <summary>
        /// Returns an equip slot with the given prototype ID, if any.
        /// </summary>
        public bool TryGetEquipSlotAndContainer(EntityUid uid, EquipSlotPrototypeId slotId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance, [NotNullWhen(true)] out ContainerSlot? containerSlot,
            InventoryComponent? inventory = null, 
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            equipSlotInstance = null;
            if (!Resolve(uid, ref inventory, ref containerComp, false))
                return false;

            if (!TryGetEquipSlot(uid, slotId, out equipSlotInstance, inventory: inventory))
                return false;

            return TryGetContainerForEquipSlot(uid, equipSlotInstance, out containerSlot, inventory, containerComp);
        }

        /// <summary>
        /// Returns the container for the given equip slot instance, creating it if necessary.
        /// </summary>
        public bool TryGetContainerForEquipSlot(EntityUid uid, EquipSlotInstance equipSlotInstance,
            [NotNullWhen(true)] out ContainerSlot? containerSlot,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            if (!Resolve(uid, ref inventory, ref containerComp, false))
                return false;

            if (!_containerSys.TryGetContainer(uid, equipSlotInstance.ContainerID, out var container))
            {
                Logger.WarningS("inv.equip", $"EquipSlot declared container ID {equipSlotInstance.ContainerID}, but it wasn't allocated yet.");
                containerSlot = _containerSys.MakeContainer<ContainerSlot>(uid, equipSlotInstance.ContainerID, containerManager: containerComp);
                containerSlot.OccludesLight = false;
                return true;
            }

            if (container is not ContainerSlot containerSlotChecked) return false;

            containerSlot = containerSlotChecked;
            return true;
        }

        /// <summary>
        /// Returns the equipment slot instance for this container, if it exists.
        /// </summary>
        public bool TryGetEquipSlotForContainer(EntityUid uid, ContainerId containerId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            InventoryComponent? inventory = null,
            ContainerManagerComponent? containerComp = null)
        {
            equipSlotInstance = null;
            if (!Resolve(uid, ref inventory, ref containerComp, false))
                return false;

            equipSlotInstance = inventory.EquipSlots.FirstOrDefault(x => x.ContainerID == containerId);
            return equipSlotInstance != default;
        }

        public bool HasEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId, InventoryComponent? inventory = null) =>
            TryGetEquipSlot(uid, slotId, out _, inventory);

        /// <summary>
        /// Returns an equipment slot with the given ID on this entity, if any.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="slotId"></param>
        /// <param name="equipSlotInstance"></param>
        /// <param name="inventory"></param>
        /// <returns></returns>
        public bool TryGetEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId, [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance, 
            InventoryComponent? inventory = null)
        {
            equipSlotInstance = null;
            if (!Resolve(uid, ref inventory, false))
                return false;

            if (!_prototypeManager.HasIndex(slotId))
                return false;

            equipSlotInstance = inventory.EquipSlots.FirstOrDefault(x => x.ID == slotId);
            return equipSlotInstance != default;
        }

        public bool TryGetContainerSlotEnumerator(EntityUid uid, out ContainerSlotEnumerator containerSlotEnumerator, 
            InventoryComponent? inventory = null)
        {
            containerSlotEnumerator = default;
            if (!Resolve(uid, ref inventory, false))
                return false;

            containerSlotEnumerator = new ContainerSlotEnumerator(uid, inventory.EquipSlots, this);
            return true;
        }

        public bool TryGetEquipSlots(EntityUid uid, [NotNullWhen(true)] out IList<EquipSlotInstance>? slotDefinitions,
            InventoryComponent? inventory = null)
        {
            slotDefinitions = null;
            if (!Resolve(uid, ref inventory, false))
                return false;

            slotDefinitions = inventory.EquipSlots;
            return true;
        }

        public IList<EquipSlotInstance> GetEquipSlots(EntityUid uid, InventoryComponent? inventory = null)
        {
            if (!Resolve(uid, ref inventory)) throw new InvalidOperationException();
            return inventory.EquipSlots;
        }

        public struct ContainerSlotEnumerator
        {
            private readonly InventorySystem _inventorySystem;
            private readonly EntityUid _uid;
            private readonly IList<EquipSlotInstance> _slots;
            private int _nextIdx = int.MaxValue;

            public ContainerSlotEnumerator(EntityUid uid, IList<EquipSlotInstance> slots, InventorySystem inventorySystem)
            {
                _uid = uid;
                _inventorySystem = inventorySystem;
                _slots = slots;
            }

            public bool MoveNext([NotNullWhen(true)] out ContainerSlot? container)
            {
                container = null;
                if (_nextIdx >= _slots.Count) return false;

                while (_nextIdx < _slots.Count && !_inventorySystem.TryGetEquipSlotAndContainer(_uid, _slots[_nextIdx++].ID, out _, out container)) { }

                return container != null;
            }
        }
    }
}

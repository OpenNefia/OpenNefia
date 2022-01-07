using OpenNefia.Content.Inventory;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using System.Diagnostics.CodeAnalysis;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;

namespace OpenNefia.Content.EquipSlots
{
    public sealed partial class EquipSlotsSystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ContainerSystem _containerSys = default!;

        // That's one beefy mutant.
        private const int MaxContainerSlotsPerEquipSlot = 1024;

        /// <inheritdoc/>
        public void InitializeEquipSlots(EntityUid uid, IEnumerable<EquipSlotPrototypeId> equipSlotProtoIds,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref equipSlotsComp, logMissing: false))
                equipSlotsComp = EntityManager.AddComponent<EquipSlotsComponent>(uid);
            if (!Resolve(uid, ref containerComp, logMissing: false))
                containerComp = EntityManager.AddComponent<ContainerManagerComponent>(uid);

            ClearEquipSlots(uid, equipSlotsComp, containerComp);

            foreach (var slotId in equipSlotProtoIds)
            {
                TryAddEquipSlot(uid, slotId, out _, out _, equipSlotsComp, containerComp);
            }
        }

        /// <inheritdoc/>
        public bool TryAddEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId,
            [NotNullWhen(true)] out ContainerSlot? containerSlot, [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            equipSlotInstance = null;
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp))
                return false;

            if (!_prototypeManager.HasIndex(slotId))
                return false;

            var containerId = FindFreeContainerIdForSlot(uid, slotId, equipSlotsComp, containerComp);
            if (containerId == null)
                return false;

            equipSlotInstance = new EquipSlotInstance(slotId, containerId.Value);
            equipSlotsComp.EquipSlots.Add(equipSlotInstance);

            containerSlot = _containerSys.MakeContainer<ContainerSlot>(uid, equipSlotInstance.ContainerID, containerManager: containerComp);
            containerSlot.OccludesLight = false;

            return true;
        }

        /// <inheritdoc/>
        public bool TryRemoveEquipSlot(EntityUid uid, EquipSlotInstance instance,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp))
                return false;

            if (!equipSlotsComp.EquipSlots.Contains(instance))
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

            equipSlotsComp.EquipSlots.Remove(instance);
            return true;
        }

        /// <inheritdoc/>
        public void ClearEquipSlots(EntityUid uid,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp))
                return;

            foreach (var slot in GetEquipSlots(uid))
            {
                TryRemoveEquipSlot(uid, slot, equipSlotsComp, containerComp);
            }

            // In case any errors occurred.
            equipSlotsComp.EquipSlots.Clear();
        }

        private static ContainerId MakeContainerId(EquipSlotPrototypeId slotId, int index)
        {
            return new($"Elona.EquipSlot:{slotId}:{index}");
        }

        private ContainerId? FindFreeContainerIdForSlot(EntityUid uid, EquipSlotPrototypeId slotId,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp))
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

        /// <inheritdoc/>
        public bool TryGetEquipSlotAndContainer(EntityUid uid, EquipSlotPrototypeId slotId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance, [NotNullWhen(true)] out ContainerSlot? containerSlot,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            equipSlotInstance = null;
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp, false))
                return false;

            if (!TryGetEquipSlot(uid, slotId, out equipSlotInstance, equipSlotsComp: equipSlotsComp))
                return false;

            return TryGetContainerForEquipSlot(uid, equipSlotInstance, out containerSlot, equipSlotsComp, containerComp);
        }

        /// <inheritdoc/>
        public bool TryGetContainerForEquipSlot(EntityUid uid, EquipSlotInstance equipSlotInstance,
            [NotNullWhen(true)] out ContainerSlot? containerSlot,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            containerSlot = null;
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp, false))
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

        /// <inheritdoc/>
        public bool TryGetEquipSlotForContainer(EntityUid uid, ContainerId containerId,
            [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null,
            ContainerManagerComponent? containerComp = null)
        {
            equipSlotInstance = null;
            if (!Resolve(uid, ref equipSlotsComp, ref containerComp, false))
                return false;

            equipSlotInstance = equipSlotsComp.EquipSlots.FirstOrDefault(x => x.ContainerID == containerId);
            return equipSlotInstance != default;
        }

        /// <inheritdoc/>
        public bool HasEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId, EquipSlotsComponent? equipSlotsComp = null) =>
            TryGetEquipSlot(uid, slotId, out _, equipSlotsComp);


        /// <inheritdoc/>
        public bool TryGetEquipSlot(EntityUid uid, EquipSlotPrototypeId slotId, [NotNullWhen(true)] out EquipSlotInstance? equipSlotInstance,
            EquipSlotsComponent? equipSlotsComp = null)
        {
            equipSlotInstance = null;
            if (!Resolve(uid, ref equipSlotsComp, false))
                return false;

            if (!_prototypeManager.HasIndex(slotId))
                return false;

            equipSlotInstance = equipSlotsComp.EquipSlots.FirstOrDefault(x => x.ID == slotId);
            return equipSlotInstance != default;
        }

        /// <inheritdoc/>
        public bool TryGetContainerSlotEnumerator(EntityUid uid, out ContainerSlotEnumerator containerSlotEnumerator,
            EquipSlotsComponent? equipSlotsComp = null)
        {
            containerSlotEnumerator = default;
            if (!Resolve(uid, ref equipSlotsComp, false))
                return false;

            containerSlotEnumerator = new ContainerSlotEnumerator(uid, equipSlotsComp.EquipSlots, this);
            return true;
        }

        /// <inheritdoc/>
        public bool TryGetEquipSlots(EntityUid uid, [NotNullWhen(true)] out IList<EquipSlotInstance>? slotDefinitions,
            EquipSlotsComponent? equipSlotsComp = null)
        {
            slotDefinitions = null;
            if (!Resolve(uid, ref equipSlotsComp, false))
                return false;

            slotDefinitions = equipSlotsComp.EquipSlots;
            return true;
        }

        /// <inheritdoc/>
        public IList<EquipSlotInstance> GetEquipSlots(EntityUid uid, EquipSlotsComponent? equipSlotsComp = null)
        {
            if (!Resolve(uid, ref equipSlotsComp)) throw new InvalidOperationException();
            return equipSlotsComp.EquipSlots;
        }

        public struct ContainerSlotEnumerator
        {
            private readonly EquipSlotsSystem _inventorySystem;
            private readonly EntityUid _uid;
            private readonly IList<EquipSlotInstance> _slots;
            private int _nextIdx = int.MaxValue;

            public ContainerSlotEnumerator(EntityUid uid, IList<EquipSlotInstance> slots, EquipSlotsSystem inventorySystem)
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

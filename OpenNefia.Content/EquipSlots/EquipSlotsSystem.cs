using Content.Shared.Inventory.Events;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots.Events;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using static OpenNefia.Content.Prototypes.Protos;
using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;

namespace OpenNefia.Content.EquipSlots
{
    /// <summary>
    /// Handles character equipment.
    /// </summary>
    /// <remarks>
    /// Based off of SS14's <c>InventorySystem</c>.
    /// </remarks>
    public sealed partial class EquipSlotsSystem : EntitySystem, IEquipSlotsSystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;

        public override void Initialize()
        {
            //these events ensure that the client also gets its proper events raised when getting its containerstate updated
            SubscribeLocalEvent<EquipSlotsComponent, EntInsertedIntoContainerMessage>(OnEntInserted, nameof(OnEntInserted));
            SubscribeLocalEvent<EquipSlotsComponent, EntRemovedFromContainerMessage>(OnEntRemoved, nameof(OnEntRemoved));
        }

        private void OnEntRemoved(EntityUid uid, EquipSlotsComponent equipSlots, EntRemovedFromContainerMessage args)
{
            if (!TryGetEquipSlotForContainer(uid, args.Container.ID, out var equipSlot, equipSlotsComp: equipSlots))
                return;

            var gotUnequippedEvent = new GotUnequippedEvent(uid, args.Entity, equipSlot);
            RaiseLocalEvent(args.Entity, gotUnequippedEvent);

            var unequippedEvent = new DidUnequipEvent(uid, args.Entity, equipSlot);
            RaiseLocalEvent(uid, unequippedEvent);
        }

        private void OnEntInserted(EntityUid uid, EquipSlotsComponent equipSlots, EntInsertedIntoContainerMessage args)
        {
            if (!TryGetEquipSlotForContainer(uid, args.Container.ID, out var equipSlot, equipSlotsComp: equipSlots))
                return;

            var gotEquippedEvent = new GotEquippedEvent(uid, args.Entity, equipSlot);
            RaiseLocalEvent(args.Entity, gotEquippedEvent);

            var equippedEvent = new DidEquipEvent(uid, args.Entity, equipSlot);
            RaiseLocalEvent(uid, equippedEvent);
        }

        public bool TryEquip(EntityUid uid, EntityUid itemUid, EquipSlotPrototypeId slot, 
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null) =>
            TryEquip(uid, uid, itemUid, slot, silent, force, equipSlots, item);

        public bool TryEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotPrototypeId slot, 
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null, 
            EquipmentComponent? item = null)
        {
            if (!Resolve(target, ref equipSlots, false) || !Resolve(itemUid, ref item, false))
            {
                if (!silent) Mes.Display(Loc.GetString("Elona.EquipSlots.CannotEquip",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)));
                return false;
            }

            if (!TryGetEquipSlotAndContainer(target, slot, out var slotDefinition, out var slotContainer, equipSlots))
            {
                if (!silent) Mes.Display(Loc.GetString("Elona.EquipSlots.CannotEquip",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)));
                return false;
            }

            if (!force && !CanEquip(actor, target, itemUid, slot, out var reason, slotDefinition, equipSlots, item))
            {
                if (!silent) Mes.Display(Loc.GetString(reason,
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)));
                return false;
            }

            if (!slotContainer.Insert(itemUid))
            {
                if (!silent) Mes.Display(Loc.GetString("Elona.EquipSlots.CannotUnequip",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)));
                return false;
            }

            if (!silent && item.EquipSound != null)
            {
                var sound = item.EquipSound.GetSound();
                if (sound != null)
                    _sounds.Play(sound.Value, target);
            }

            return true;
        }

        public bool CanEquip(EntityUid uid, EntityUid itemUid, EquipSlotPrototypeId slot, 
            [NotNullWhen(false)] out string? reason,
            EquipSlotInstance? equipSlot = null, EquipSlotsComponent? inventory = null,
            EquipmentComponent? item = null) =>
            CanEquip(uid, uid, itemUid, slot, out reason, equipSlot, inventory, item);

        public bool CanEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotPrototypeId slot, 
            [NotNullWhen(false)] out string? reason, 
            EquipSlotInstance? equipSlot = null, 
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null)
        {
            reason = "Elona.EquipSlots.CannotEquip";
            if (!Resolve(target, ref equipSlots, false) || !Resolve(itemUid, ref item, false))
                return false;

            if (!item.EquipSlots.Contains(slot))
                return false;

            if (equipSlot == null && !TryGetEquipSlot(target, slot, out equipSlot, equipSlotsComp: equipSlots))
                return false;
           
            var attemptEvent = new IsEquippingAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseLocalEvent(target, attemptEvent);
            if (attemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            if (actor != target)
            {
                //reuse the event. this is gucci, right?
                attemptEvent.Reason = null;
                RaiseLocalEvent(actor, attemptEvent);
                if (attemptEvent.Cancelled)
                {
                    reason = attemptEvent.Reason ?? reason;
                    return false;
                }
            }

            var itemAttemptEvent = new BeingEquippedAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseLocalEvent(itemUid, itemAttemptEvent);
            if (itemAttemptEvent.Cancelled)
            {
                reason = itemAttemptEvent.Reason ?? reason;
                return false;
            }

            return true;
        }

        public bool TryUnequip(EntityUid uid, EquipSlotPrototypeId slot, 
            bool silent = false, bool force = false,
            EquipSlotsComponent? inventory = null) => 
            TryUnequip(uid, uid, slot, silent, force, inventory);

        public bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotPrototypeId slot,
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null) =>
            TryUnequip(actor, target, slot, out _, silent, force, equipSlots);

        public bool TryUnequip(EntityUid uid, EquipSlotPrototypeId slot, [NotNullWhen(true)] out EntityUid? removedItem, 
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null) 
            => TryUnequip(uid, uid, slot, out removedItem, silent, force, equipSlots);

        public bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotPrototypeId slot, 
            [NotNullWhen(true)] out EntityUid? removedItem, 
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null)
        {
            removedItem = null;
            if (!Resolve(target, ref equipSlots, false))
            {
                if (!silent) Mes.Display(Loc.GetString("Elona.EquipSlots.CannotUnequip",
                    ("actor", actor),
                    ("target", target),
                    ("item", EntityUid.Invalid)));
                return false;
            }

            if (!TryGetEquipSlotAndContainer(target, slot, out var slotDefinition, out var slotContainer, equipSlots))
            {
                if (!silent) Mes.Display(Loc.GetString("Elona.EquipSlots.CannotUnequip",
                    ("actor", actor),
                    ("target", target),
                    ("item", EntityUid.Invalid)));
                return false;
            }

            removedItem = slotContainer.ContainedEntity;

            if (!removedItem.HasValue) return false;

            if (!force && !CanUnequip(actor, target, slot, out var reason, slotContainer, slotDefinition, equipSlots))
            {
                if (!silent) Mes.Display(Loc.GetString(reason, 
                    ("actor", actor),
                    ("target", target), 
                    ("item", slotContainer.ContainedEntity)));
                return false;
            }

            //we need to do this to make sure we are 100% removing this entity, since we are now dropping dependant slots
            if (!force && !slotContainer.CanRemove(removedItem.Value))
                return false;

            if (force)
            {
                slotContainer.ForceRemove(removedItem.Value);
            }
            else
            {
                if (!slotContainer.Remove(removedItem.Value))
                {
                    //should never happen bc of the canremove lets just keep in just in case
                    Logger.WarningS("equip", $"Could not remove unequipped entity {removedItem.Value} from container {slotContainer.ID}!");
                    return false;
                }
            }

            EntityManager.GetComponent<SpatialComponent>(removedItem.Value).Coordinates =
                EntityManager.GetComponent<SpatialComponent>(target).Coordinates;

            return true;
        }

        public bool CanUnequip(EntityUid uid, EquipSlotPrototypeId slot, [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null, EquipSlotInstance? equipSlot = null,
            EquipSlotsComponent? equipSlots = null) =>
            CanUnequip(uid, uid, slot, out reason, containerSlot, equipSlot, equipSlots);

        public bool CanUnequip(EntityUid actor, EntityUid target, EquipSlotPrototypeId slot, 
            [NotNullWhen(false)] out string? reason, 
            ContainerSlot? containerSlot = null,
            EquipSlotInstance? equipSlot = null, 
            EquipSlotsComponent? equipSlots = null)
        {
            reason = "Elona.EquipSlots.CannotUnequip";
            if (!Resolve(target, ref equipSlots, false))
                return false;

            if ((containerSlot == null || equipSlot == null) && !TryGetEquipSlotAndContainer(target, slot, out equipSlot, out containerSlot, equipSlots))
                return false;

            if (containerSlot.ContainedEntity == null || !containerSlot.CanRemove(containerSlot.ContainedEntity.Value))
                return false;

            var itemUid = containerSlot.ContainedEntity.Value;

            var attemptEvent = new IsUnequippingAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseLocalEvent(target, attemptEvent);
            if (attemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            if (actor != target)
            {
                //reuse the event. this is gucci, right?
                attemptEvent.Reason = null;
                RaiseLocalEvent(actor, attemptEvent);
                if (attemptEvent.Cancelled)
                {
                    reason = attemptEvent.Reason ?? reason;
                    return false;
                }
            }

            var itemAttemptEvent = new BeingUnequippedAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseLocalEvent(itemUid, itemAttemptEvent);
            if (itemAttemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            return true;
        }

        public bool TryGetSlotEntity(EntityUid uid, EquipSlotPrototypeId slot, [NotNullWhen(true)] out EntityUid? entityUid, 
            EquipSlotsComponent? equipSlotsComponent = null, 
            ContainerManagerComponent? containerManagerComponent = null)
        {
            entityUid = null;
            if (!Resolve(uid, ref equipSlotsComponent, ref containerManagerComponent, false)
                || !TryGetEquipSlotAndContainer(uid, slot, out _, out var container, equipSlotsComponent, containerManagerComponent))
                return false;

            entityUid = container.ContainedEntity;
            return entityUid != null;
        }
    }
}

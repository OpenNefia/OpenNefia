using Content.Shared.Inventory.Events;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots.Events;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
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
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
            SubscribeComponent<EquipSlotsComponent, EntityBeingGeneratedEvent>(BeingGenerated_InitializeEquipSlots, priority: EventPriorities.Highest);

            //these events ensure that the client also gets its proper events raised when getting its containerstate updated
            SubscribeComponent<EquipSlotsComponent, EntInsertedIntoContainerEventArgs>(OnEntInserted, priority: EventPriorities.Low);
            SubscribeComponent<EquipSlotsComponent, EntRemovedFromContainerEventArgs>(OnEntRemoved, priority: EventPriorities.Low);
        }

        private void BeingGenerated_InitializeEquipSlots(EntityUid uid, EquipSlotsComponent component, ref EntityBeingGeneratedEvent args)
        {
            var list = new List<EquipSlotPrototypeId>();
            list.AddRange(component.InitialEquipSlots);
            var ev = new GetInitialEquipSlotsEvent(list);
            RaiseEvent(uid, ev);

            InitializeEquipSlots(uid, ev.OutEquipSlots, component);
        }

        private void OnEntRemoved(EntityUid uid, EquipSlotsComponent equipSlots, EntRemovedFromContainerEventArgs args)
        {
            if (!TryGetEquipSlotForContainer(uid, args.Container.ID, out var equipSlot, equipSlotsComp: equipSlots))
                return;

            var gotUnequippedEvent = new GotUnequippedEvent(uid, args.Entity, equipSlot);
            RaiseEvent(args.Entity, gotUnequippedEvent);

            var unequippedEvent = new DidUnequipEvent(uid, args.Entity, equipSlot);
            RaiseEvent(uid, unequippedEvent);
        }

        private void OnEntInserted(EntityUid uid, EquipSlotsComponent equipSlots, EntInsertedIntoContainerEventArgs args)
        {
            if (!TryGetEquipSlotForContainer(uid, args.Container.ID, out var equipSlot, equipSlotsComp: equipSlots))
                return;

            var gotEquippedEvent = new GotEquippedEvent(uid, args.Entity, equipSlot);
            RaiseEvent(args.Entity, gotEquippedEvent);

            var equippedEvent = new DidEquipEvent(uid, args.Entity, equipSlot);
            RaiseEvent(uid, equippedEvent);
        }

        public bool TryEquip(EntityUid uid, EntityUid itemUid, EquipSlotInstance equipSlot,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null) =>
            TryEquip(uid, uid, itemUid, equipSlot, silent, force, equipSlots, item);

        public bool TryEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotInstance equipSlot,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null)
        {
            if (!Resolve(target, ref equipSlots, false) || !Resolve(itemUid, ref item, false))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Equip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
                return false;
            }

            if (!TryGetContainerForEquipSlot(target, equipSlot, out var slotContainer, equipSlots))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Equip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
                return false;
            }

            if (!_stacks.TrySplit(itemUid, 1, out var split))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Equip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
                return false;
            }

            itemUid = split;

            if (!force && !CanEquip(actor, target, itemUid, equipSlot, out var reason, equipSlots, item))
            {
                if (!silent) _mes.Display(Loc.GetString(reason,
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
                return false;
            }

            if (!slotContainer.Insert(itemUid))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Equip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
                return false;
            }

            if (!silent)
            {
                if (item.EquipSound != null)
                {
                    var sound = item.EquipSound.GetSound();
                    if (sound != null)
                        _sounds.Play(sound.Value, target);
                }

                _mes.Newline();
                _mes.Display(Loc.GetString("Elona.EquipSlots.Equip.Succeeds",
                    ("actor", actor),
                    ("target", target),
                    ("item", itemUid)),
                    entity: target);
            }

            return true;
        }

        public bool CanEquip(EntityUid uid, EntityUid itemUid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            EquipSlotsComponent? inventory = null,
            EquipmentComponent? item = null) =>
            CanEquip(uid, uid, itemUid, equipSlot, out reason, inventory, item);

        public bool CanEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null)
        {
            reason = "Elona.EquipSlots.Equip.Fails";
            if (!Resolve(target, ref equipSlots, false) || !Resolve(itemUid, ref item, false))
                return false;

            if (!item.EquipSlots.Contains(equipSlot.ID))
                return false;

            var attemptEvent = new IsEquippingAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseEvent(target, attemptEvent);
            if (attemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            if (actor != target)
            {
                //reuse the event. this is gucci, right?
                attemptEvent.Reason = null;
                RaiseEvent(actor, attemptEvent);
                if (attemptEvent.Cancelled)
                {
                    reason = attemptEvent.Reason ?? reason;
                    return false;
                }
            }

            var itemAttemptEvent = new BeingEquippedAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseEvent(itemUid, itemAttemptEvent);
            if (itemAttemptEvent.Cancelled)
            {
                reason = itemAttemptEvent.Reason ?? reason;
                return false;
            }

            return true;
        }

        public bool TryUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? inventory = null) =>
            TryUnequip(uid, uid, equipSlot, placeInto, silent, force, inventory);

        public bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null) =>
            TryUnequip(actor, target, equipSlot, out _, placeInto, silent, force, equipSlots);

        public bool TryUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            [NotNullWhen(true)] out EntityUid? removedItem,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null)
            => TryUnequip(uid, uid, equipSlot, out removedItem, placeInto, silent, force, equipSlots);

        public bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            [NotNullWhen(true)] out EntityUid? removedItem,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null)
        {
            removedItem = null;
            if (!Resolve(target, ref equipSlots, false))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Unequip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", EntityUid.Invalid)),
                    entity: target);
                return false;
            }

            if (!TryGetContainerForEquipSlot(target, equipSlot, out var slotContainer, equipSlots))
            {
                if (!silent) _mes.Display(Loc.GetString("Elona.EquipSlots.Unequip.Fails",
                    ("actor", actor),
                    ("target", target),
                    ("item", EntityUid.Invalid)),
                    entity: target);
                return false;
            }

            removedItem = slotContainer.ContainedEntity;

            if (!removedItem.HasValue) return false;

            if (!force && !CanUnequip(actor, target, equipSlot, out var reason, slotContainer, equipSlots))
            {
                if (!silent) _mes.Display(Loc.GetString(reason,
                    ("actor", actor),
                    ("target", target),
                    ("item", slotContainer.ContainedEntity)),
                    entity: target);
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

            ReparentUnequippedEntity(target, removedItem.Value, placeInto);

            if (!silent)
            {
                _mes.Newline();
                _mes.Display(Loc.GetString("Elona.EquipSlots.Unequip.Succeeds",
                    ("actor", actor),
                    ("target", target),
                    ("item", removedItem.Value)),
                    entity: target);
            }

            return true;
        }

        private void ReparentUnequippedEntity(EntityUid target, EntityUid unequipped,
            IContainer? placeInto,
            SpatialComponent? targetSpatial = null,
            SpatialComponent? unequippedSpatial = null)
        {
            if (!Resolve(target, ref targetSpatial) || !Resolve(unequipped, ref unequippedSpatial))
                return;

            // Move the item to the container, if it was passed.
            if (placeInto != null && placeInto.Insert(unequipped))
                return;

            // Place the item in the parent of the equipper (usually the map, but could be inside
            // a nested container).
            _containerSys.AttachParentToContainerOrMap(unequippedSpatial);
        }

        public bool CanUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null,
            EquipSlotsComponent? equipSlots = null) =>
            CanUnequip(uid, uid, equipSlot, out reason, containerSlot, equipSlots);

        public bool CanUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null,
            EquipSlotsComponent? equipSlots = null)
        {
            reason = "Elona.EquipSlots.Unequip.Fails";
            if (!Resolve(target, ref equipSlots, false))
                return false;

            if (containerSlot == null && !TryGetContainerForEquipSlot(target, equipSlot, out containerSlot, equipSlots))
                return false;

            if (containerSlot.ContainedEntity == null || !containerSlot.CanRemove(containerSlot.ContainedEntity.Value))
                return false;

            var itemUid = containerSlot.ContainedEntity.Value;

            var attemptEvent = new IsUnequippingAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseEvent(target, attemptEvent);
            if (attemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            if (actor != target)
            {
                //reuse the event. this is gucci, right?
                attemptEvent.Reason = null;
                RaiseEvent(actor, attemptEvent);
                if (attemptEvent.Cancelled)
                {
                    reason = attemptEvent.Reason ?? reason;
                    return false;
                }
            }

            var itemAttemptEvent = new BeingUnequippedAttemptEvent(actor, target, itemUid, equipSlot);
            RaiseEvent(itemUid, itemAttemptEvent);
            if (itemAttemptEvent.Cancelled)
            {
                reason = attemptEvent.Reason ?? reason;
                return false;
            }

            return true;
        }

        public bool TryGetSlotEntity(EntityUid uid, EquipSlotInstance equipSlot, [NotNullWhen(true)] out EntityUid? entityUid,
            EquipSlotsComponent? equipSlotsComponent = null,
            ContainerManagerComponent? containerManagerComponent = null)
        {
            entityUid = null;
            if (!Resolve(uid, ref equipSlotsComponent, ref containerManagerComponent, false)
                || !TryGetContainerForEquipSlot(uid, equipSlot, out var container, equipSlotsComponent, containerManagerComponent))
                return false;

            entityUid = container.ContainedEntity;
            return entityUid != null;
        }
    }

    public sealed class GetInitialEquipSlotsEvent : EntityEventArgs
    {
        public List<EquipSlotPrototypeId> OutEquipSlots { get; }

        public GetInitialEquipSlotsEvent(List<EquipSlotPrototypeId> outEquipSlots)
        {
            OutEquipSlots = outEquipSlots;
        }
    }
}

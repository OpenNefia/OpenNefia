using OpenNefia.Content.Equipment;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.EquipSlots
{
    public partial interface IEquipSlotsSystem : IEntitySystem
    {
        bool CanEquip(EntityUid uid, EntityUid itemUid, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(false)] out string? reason, 
            EquipSlotInstance? equipSlot = null,
            EquipSlotsComponent? inventory = null, 
            EquipmentComponent? item = null);

        bool CanEquip(EntityUid actor, EntityUid target, EntityUid itemUid, PrototypeId<EquipSlotPrototype> slot,
            [NotNullWhen(false)] out string? reason,
            EquipSlotInstance? equipSlot = null, 
            EquipSlotsComponent? equipSlots = null, 
            EquipmentComponent? item = null);

        bool TryEquip(EntityUid uid, EntityUid itemUid, PrototypeId<EquipSlotPrototype> slot, 
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null);

        bool TryEquip(EntityUid actor, EntityUid target, EntityUid itemUid, PrototypeId<EquipSlotPrototype> slot,
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null, 
            EquipmentComponent? item = null);

        bool CanUnequip(EntityUid uid, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(false)] out string? reason, 
            ContainerSlot? containerSlot = null, 
            EquipSlotInstance? equipSlot = null, 
            EquipSlotsComponent? equipSlots = null);

        bool CanUnequip(EntityUid actor, EntityUid target, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null,
            EquipSlotInstance? equipSlot = null, 
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid uid, PrototypeId<EquipSlotPrototype> slot,
            bool silent = false, bool force = false, 
            EquipSlotsComponent? inventory = null);

        bool TryUnequip(EntityUid actor, EntityUid target, PrototypeId<EquipSlotPrototype> slot,
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid uid, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(true)] out EntityUid? removedItem, 
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid actor, EntityUid target, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(true)] out EntityUid? removedItem, 
            bool silent = false, bool force = false, 
            EquipSlotsComponent? equipSlots = null);

        bool TryGetSlotEntity(EntityUid uid, PrototypeId<EquipSlotPrototype> slot, 
            [NotNullWhen(true)] out EntityUid? entityUid,
            EquipSlotsComponent? equipSlotsComponent = null,
            ContainerManagerComponent? containerManagerComponent = null);
    }
}
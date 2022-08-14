using OpenNefia.Content.Equipment;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.EquipSlots
{
    public partial interface IEquipSlotsSystem : IEntitySystem
    {
        bool CanEquip(EntityUid uid, EntityUid itemUid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            EquipSlotsComponent? inventory = null,
            EquipmentComponent? item = null);

        bool CanEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null);

        bool TryEquip(EntityUid uid, EntityUid itemUid, EquipSlotInstance equipSlot,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null);

        bool TryEquip(EntityUid actor, EntityUid target, EntityUid itemUid, EquipSlotInstance equipSlot,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null,
            EquipmentComponent? item = null);

        bool CanUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null,
            EquipSlotsComponent? equipSlots = null);

        bool CanUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            [NotNullWhen(false)] out string? reason,
            ContainerSlot? containerSlot = null,
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? inventory = null);

        bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid uid, EquipSlotInstance equipSlot,
            [NotNullWhen(true)] out EntityUid? removedItem,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null);

        bool TryUnequip(EntityUid actor, EntityUid target, EquipSlotInstance equipSlot,
            [NotNullWhen(true)] out EntityUid? removedItem,
            IContainer? placeInto = null,
            bool silent = false, bool force = false,
            EquipSlotsComponent? equipSlots = null);

        bool TryGetSlotEntity(EntityUid uid, EquipSlotInstance equipSlot,
            [NotNullWhen(true)] out EntityUid? entityUid,
            EquipSlotsComponent? equipSlotsComponent = null,
            ContainerManagerComponent? containerManagerComponent = null);
    }
}
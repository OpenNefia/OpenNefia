using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.EquipSlots.Events;

public class UnequipAttemptEventBase : CancellableEntityEventArgs
{
    /// <summary>
    /// The entity performing the action. NOT necessarily the same as the entity whose equipment is being removed..
    /// </summary>
    public readonly EntityUid Unequipee;

    /// <summary>
    /// The entity being unequipped from.
    /// </summary>
    public readonly EntityUid UnEquipTarget;

    /// <summary>
    /// The entity to be unequipped.
    /// </summary>
    public readonly EntityUid Equipment;

    /// <summary>
    /// The slot the entity is being unequipped from.
    /// </summary>
    public readonly EquipSlotInstance EquipSlot;

    /// <summary>
    /// If cancelling and wanting to provide a custom reason, use this field. Not that this expects a loc-id.
    /// </summary>
    public string? Reason;

    public UnequipAttemptEventBase(EntityUid unequipee, EntityUid unEquipTarget, EntityUid equipment,
        EquipSlotInstance equipSlot)
    {
        UnEquipTarget = unEquipTarget;
        Equipment = equipment;
        Unequipee = unequipee;
        EquipSlot = equipSlot;
    }
}

public class BeingUnequippedAttemptEvent : UnequipAttemptEventBase
{
    public BeingUnequippedAttemptEvent(EntityUid unequipee, EntityUid unEquipTarget, EntityUid equipment,
        EquipSlotInstance slotDefinition) : base(unequipee, unEquipTarget, equipment, slotDefinition)
    {
    }
}

public class IsUnequippingAttemptEvent : UnequipAttemptEventBase
{
    public IsUnequippingAttemptEvent(EntityUid unequipee, EntityUid unEquipTarget, EntityUid equipment,
        EquipSlotInstance slotDefinition) : base(unequipee, unEquipTarget, equipment, slotDefinition)
    {
    }
}

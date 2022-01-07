using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.EquipSlots.Events;

public abstract class EquipAttemptBase : CancellableEntityEventArgs
{
    /// <summary>
    /// The entity performing the action. NOT necessarily the one actually "receiving" the equipment.
    /// </summary>
    public readonly EntityUid Equipee;

    /// <summary>
    /// The entity being equipped to.
    /// </summary>
    public readonly EntityUid EquipTarget;

    /// <summary>
    /// The entity to be equipped.
    /// </summary>
    public readonly EntityUid Equipment;

    /// <summary>
    /// The slot the entity is being equipped to.
    /// </summary>
    public readonly EquipSlotInstance EquipSlot;

    /// <summary>
    /// If cancelling and wanting to provide a custom reason, use this field. Not that this expects a loc-id.
    /// </summary>
    public string? Reason;

    public EquipAttemptBase(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        EquipSlotInstance equipSlot)
    {
        EquipTarget = equipTarget;
        Equipment = equipment;
        Equipee = equipee;
        EquipSlot = equipSlot;
    }
}

public class BeingEquippedAttemptEvent : EquipAttemptBase
{
    public BeingEquippedAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        EquipSlotInstance equipSlot) : base(equipee, equipTarget, equipment, equipSlot)
    {
    }
}

public class IsEquippingAttemptEvent : EquipAttemptBase
{
    public IsEquippingAttemptEvent(EntityUid equipee, EntityUid equipTarget, EntityUid equipment,
        EquipSlotInstance equipSlot) : base(equipee, equipTarget, equipment, equipSlot)
    {
    }
}

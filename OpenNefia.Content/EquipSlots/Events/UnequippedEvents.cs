using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.EquipSlots.Events;

public class UnequippedEventBase : EntityEventArgs
{
    /// <summary>
    /// The entity unequipping.
    /// </summary>
    public readonly EntityUid Equipee;

    /// <summary>
    /// The entity which got unequipped.
    /// </summary>
    public readonly EntityUid Equipment;

    /// <summary>
    /// The slot the entity got unequipped from.
    /// </summary>
    public readonly EquipSlotInstance EquipSlot;

    public UnequippedEventBase(EntityUid equipee, EntityUid equipment, EquipSlotInstance equipSlot)
    {
        Equipee = equipee;
        Equipment = equipment;
        EquipSlot = equipSlot;
    }
}

public class DidUnequipEvent : UnequippedEventBase
{
    public DidUnequipEvent(EntityUid equipee, EntityUid equipment, EquipSlotInstance slotDefinition) : base(equipee, equipment, slotDefinition)
    {
    }
}

public class GotUnequippedEvent : UnequippedEventBase
{
    public GotUnequippedEvent(EntityUid equipee, EntityUid equipment, EquipSlotInstance slotDefinition) : base(equipee, equipment, slotDefinition)
    {
    }
}

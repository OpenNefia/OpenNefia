using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.Shared.Inventory.Events
{
    public class EquippedEventBase : EntityEventArgs
    {
        /// <summary>
        /// The entity equipping.
        /// </summary>
        public readonly EntityUid Equipee;

        /// <summary>
        /// The entity which got equipped.
        /// </summary>
        public readonly EntityUid Equipment;

        /// <summary>
        /// The slot the entity got equipped to.
        /// </summary>
        public readonly EquipSlotInstance EquipSlot;

        public EquippedEventBase(EntityUid equipee, EntityUid equipment, EquipSlotInstance equipSlot)
        {
            Equipee = equipee;
            Equipment = equipment;
            EquipSlot = equipSlot;
        }
    }

    public class DidEquipEvent : EquippedEventBase
    {
        public DidEquipEvent(EntityUid equipee, EntityUid equipment, EquipSlotInstance equipSlot) : base(equipee, equipment, equipSlot)
        {
        }
    }

    public class GotEquippedEvent : EquippedEventBase
    {
        public GotEquippedEvent(EntityUid equipee, EntityUid equipment, EquipSlotInstance equipSlot) : base(equipee, equipment, equipSlot)
        {
        }
    }
}
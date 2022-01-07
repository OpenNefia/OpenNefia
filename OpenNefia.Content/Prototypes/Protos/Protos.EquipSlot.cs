using EquipSlotPrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.EquipSlots.EquipSlotPrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class EquipSlot
        {
            public static readonly EquipSlotPrototypeId Head = new($"Elona.{nameof(Head)}");
            public static readonly EquipSlotPrototypeId Neck = new($"Elona.{nameof(Neck)}");
            public static readonly EquipSlotPrototypeId Back = new($"Elona.{nameof(Back)}");
            public static readonly EquipSlotPrototypeId Body = new($"Elona.{nameof(Body)}");
            public static readonly EquipSlotPrototypeId Hand = new($"Elona.{nameof(Hand)}");
            public static readonly EquipSlotPrototypeId Ring = new($"Elona.{nameof(Ring)}");
            public static readonly EquipSlotPrototypeId Arm = new($"Elona.{nameof(Arm)}");
            public static readonly EquipSlotPrototypeId Waist = new($"Elona.{nameof(Waist)}");
            public static readonly EquipSlotPrototypeId Leg = new($"Elona.{nameof(Leg)}");
            public static readonly EquipSlotPrototypeId Ranged = new($"Elona.{nameof(Ranged)}");
            public static readonly EquipSlotPrototypeId Ammo = new($"Elona.{nameof(Ammo)}");
        }
    }
}

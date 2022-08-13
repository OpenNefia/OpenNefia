using EquipmentTypePrototypeId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.Equipment.EquipmentTypePrototype>;

namespace OpenNefia.Content.Prototypes
{
    public static partial class Protos
    {
        public static class EquipmentType
        {
            #pragma warning disable format

            public static readonly EquipmentTypePrototypeId Warrior  = new($"Elona.{nameof(Warrior)}");
            public static readonly EquipmentTypePrototypeId Mage     = new($"Elona.{nameof(Mage)}");
            public static readonly EquipmentTypePrototypeId Archer   = new($"Elona.{nameof(Archer)}");
            public static readonly EquipmentTypePrototypeId Gunner   = new($"Elona.{nameof(Gunner)}");
            public static readonly EquipmentTypePrototypeId WarMage  = new($"Elona.{nameof(WarMage)}");
            public static readonly EquipmentTypePrototypeId Priest   = new($"Elona.{nameof(Priest)}");
            public static readonly EquipmentTypePrototypeId Thief    = new($"Elona.{nameof(Thief)}");
            public static readonly EquipmentTypePrototypeId Claymore = new($"Elona.{nameof(Claymore)}");

            #pragma warning restore format
        }
    }
}

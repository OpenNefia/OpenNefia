using OpenNefia.Content.EquipSlots;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.LightenedTravels.Prototypes
{
    public static class Protos
    {
        public static class Item
        {
            public static readonly PrototypeId<EntityPrototype> FueledTorch = new($"LightenedTravels.Item{nameof(FueledTorch)}");
        }
        
        public static class EquipSlot
        {
            public static readonly PrototypeId<EquipSlotPrototype> Light = new($"LightenedTravels.{nameof(Light)}");
        }
    }
}

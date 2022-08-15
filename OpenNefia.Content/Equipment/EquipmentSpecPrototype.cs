using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Equipment
{
    [Prototype("Elona.EquipmentSpec")]
    public class EquipmentSpecPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("validEquipSlots")]
        public HashSet<PrototypeId<EquipSlotPrototype>> _validEquipSlots { get; } = new();
        public IReadOnlySet<PrototypeId<EquipSlotPrototype>> ValidEquipSlots => _validEquipSlots;

        [DataField]
        public int? MaxEquipSlotsToApplyTo { get; set; } = 1;
    }
}
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Equipment
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EquipmentGenComponent : Component
    {
        [DataField]
        public PrototypeId<EquipmentTypePrototype>? EquipmentType { get; set; }

        [DataField("initialEquipment")]
        private Dictionary<PrototypeId<EquipmentSpecPrototype>, InitialEquipmentEntry> _initialEquipment = new();

        /// <summary>
        /// Describes the initial equipment for a character. Applied *after* the
        /// equipment spec is generated, so specifiers here override those in
        /// the <see cref="EquipmentSpecPrototype"/> events.
        /// </summary>
        public IReadOnlyDictionary<PrototypeId<EquipmentSpecPrototype>, InitialEquipmentEntry> InitialEquipment => _initialEquipment;
    }
}
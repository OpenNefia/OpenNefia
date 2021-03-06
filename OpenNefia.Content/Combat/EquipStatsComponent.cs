using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EquipStatsComponent : Component
    {
        public override string Name => "EquipStats";

        /// <summary>
        /// Added Defense Value.
        /// </summary>
        [DataField]
        public Stat<int> DV { get; set; } = new(0);

        /// <summary>
        /// Added Protection Value.
        /// </summary>
        [DataField]
        public Stat<int> PV { get; set; } = new(0);

        /// <summary>
        /// Added hit bonus.
        /// </summary>
        [DataField]
        public Stat<int> HitBonus { get; set; } = new(0);

        /// <summary>
        /// Added damage bonus.
        /// </summary>
        [DataField]
        public Stat<int> DamageBonus { get; set; } = new(0);

        /// <summary>
        /// Added pierce rate.
        /// </summary>
        [DataField]
        public Stat<int> PierceRate { get; set; } = new(0);

        /// <summary>
        /// Added critical rate.
        /// </summary>
        [DataField]
        public Stat<int> CriticalRate { get; set; } = new(0);

        /// <summary>
        /// Added damage resistance.
        /// </summary>
        [DataField]
        public Stat<int> DamageResistance { get; set; } = new(0);
    }
}
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using OpenNefia.Content.Skills;
using System.Collections.Generic;

namespace OpenNefia.Content.Equipment
{
    /// <summary>
    /// Extra equipment bonuses to add to a wielder with a <see cref="SkillsComponent"/>
    /// </summary>
    [RegisterComponent]
    public class EquipBonusComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "EquipBonus";

        /// <summary>
        /// Added Defense Value.
        /// </summary>
        [DataField]
        public Stat<int> DV { get; set; } = 0;

        /// <summary>
        /// Added Protection Value.
        /// </summary>
        [DataField]
        public Stat<int> PV { get; set; } = 0;

        /// <summary>
        /// Added hit bonus.
        /// </summary>
        [DataField]
        public Stat<int> HitBonus { get; set; } = 0;

        /// <summary>
        /// Added damage bonus.
        /// </summary>
        [DataField]
        public Stat<int> DamageBonus { get; set; } = 0;
    }
}

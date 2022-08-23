using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EquipStatsComponent : Component, IComponentRefreshable
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

        /// <summary>
        /// Added damage immunity rate.
        /// </summary>
        [DataField]
        public Stat<float> DamageImmunityRate { get; set; } = new(0);

        /// <summary>
        /// Added damage reflection.
        /// </summary>
        [DataField]
        public Stat<float> DamageReflection { get; set; } = new(0);

        /// <summary>
        /// Added damage reflection.
        /// </summary>
        [DataField]
        public Stat<float> ExtraMeleeAttackRate { get; set; } = new(0);

        /// <summary>
        /// Added damage reflection.
        /// </summary>
        [DataField]
        public Stat<float> ExtraRangedAttackRate { get; set; } = new(0);

        public void Refresh()
        {
            DV.Reset();
            PV.Reset();
            HitBonus.Reset();
            DamageBonus.Reset();
            PierceRate.Reset();
            CriticalRate.Reset();
            DamageResistance.Reset();
            DamageImmunityRate.Reset();
            DamageReflection.Reset();
            ExtraMeleeAttackRate.Reset();
            ExtraRangedAttackRate.Reset();
        }
    }
}
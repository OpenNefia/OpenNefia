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
    public sealed class DamageImmunityComponent : Component
    {
        public override string Name => "DamageImmunity";

        [[DataField]
        public Stat<float> DamageImmunityChance { get; set; } = new(0f);
    }
}
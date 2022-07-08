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
    public sealed class NullifyDamageComponent : Component
    {
        public override string Name => "NullifyDamage";

        [DataField]
        public Stat<float> NullifyDamageChance { get; set; } = new(0f);
    }
}
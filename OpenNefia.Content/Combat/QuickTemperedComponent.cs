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
    public sealed class QuickTemperedComponent : Component
    {
        public override string Name => "QuickTempered";

        [DataField]
        public Stat<bool> IsQuickTempered { get; set; } = new(true);

        [DataField]
        public Stat<float> EnrageChance { get; set; } = new(0.05f);
    }
}
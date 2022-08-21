using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ValueComponent : Component
    {
        public override string Name => "Value";

        [DataField]
        public Stat<int> Value { get; set; } = new(0);
    }
}
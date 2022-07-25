using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class InstrumentComponent : Component
    {
        public override string Name => "Instrument";

        [DataField]
        public int PerformanceQuality { get; set; } = 0;
    }
}
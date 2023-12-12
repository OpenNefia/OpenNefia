using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items.Impl
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class WellComponent : Component
    {
        [DataField]
        public int WaterAmount { get; set; } = 0;

        [DataField]
        public int DrynessAmount { get; set; } = 0;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class HolyWellComponent : Component
    {
        [DataField]
        public int WaterAmount { get; set; } = 0;
    }
}
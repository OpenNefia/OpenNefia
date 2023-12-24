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
    public sealed class NuclearBombComponent : Component
    {
        [DataField]
        public int Power { get; set; } = 100;
    }
}
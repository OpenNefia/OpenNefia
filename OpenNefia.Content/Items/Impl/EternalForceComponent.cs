using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EternalForceComponent : Component
    {
        public override string Name => "EternalForce";

        [DataField]
        public Stat<bool> IsEternalForce { get; set; } = new(true);
    }
}
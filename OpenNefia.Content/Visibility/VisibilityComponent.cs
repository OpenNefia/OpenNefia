using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Visibility
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class VisibilityComponent : Component
    {
        public override string Name => "Visibility";

        [DataField]
        public Stat<bool> IsInvisible { get; set; } = new(false);

        [DataField]
        public Stat<bool> CanSeeInvisible { get; set; } = new(false);
    }
}
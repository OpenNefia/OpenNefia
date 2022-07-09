using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.InUse
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class InUseComponent : Component
    {
        public override string Name => "InUse";

        [DataField(required: true)]
        public EntityUid User { get; internal set; }
    }
}
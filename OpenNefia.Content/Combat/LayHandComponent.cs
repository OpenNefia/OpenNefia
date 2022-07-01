using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class LayHandComponent : Component
    {
        public override string Name => "LayHand";

        [DataField]
        public bool HasLayHand { get; set; } = false;
    }
}
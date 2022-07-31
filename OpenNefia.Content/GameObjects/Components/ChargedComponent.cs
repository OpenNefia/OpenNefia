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
    public sealed class ChargedComponent : Component
    {
        public override string Name => "Charged";

        [DataField]
        public int Charges { get; set; }

        [DataField]
        public int MaxCharges { get; set; }

        [DataField]
        public bool CanBeRecharged { get; set; }
    }
}
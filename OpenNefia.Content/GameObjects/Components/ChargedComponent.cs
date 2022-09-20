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
        [DataField]
        public int Charges { get; set; }

        [DataField]
        public int MaxCharges { get; set; } = 1;

        [DataField]
        public bool CanBeRecharged { get; set; } = true;

        [DataField]
        public bool DisplayChargeCount { get; set; } = true;
    }
}
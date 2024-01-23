using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Formulae;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Chargeable
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ChargeableComponent : Component
    {
        [DataField]
        public int Charges { get; set; }

        [DataField]
        public Formula InitialCharges { get; set; } = new("maxCharges");

        [DataField]
        public int MaxCharges { get; set; } = 1;

        [DataField]
        public bool CanBeRecharged { get; set; } = true;

        [DataField]
        public bool DisplayChargeCount { get; set; } = true;

        [DataField]
        public int RechargeCost { get; set; } = 0;
    }
}
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
    public sealed class MoneyBoxComponent : Component
    {
        [DataField]
        public int GoldDeposited { get; set; }

        [DataField]
        public int GoldIncrement { get; set; }

        [DataField]
        public int GoldLimit { get; set; } = 1000000000;
    }
}
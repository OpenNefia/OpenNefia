using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class HarvestedFoodComponent : Component
    {
        public override string Name => "HarvestedFood";
        
        /// <summary>
        /// 0-9.
        /// </summary>
        [DataField]
        public int WeightClass { get; set; }
    }
}
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
    public sealed class FurnitureComponent : Component
    {
        /// <summary>
        /// Quality of the furniture; used for calculating its monetary value.
        /// Typically between 0-12.
        /// </summary>
        [DataField]
        public int FurnitureQuality { get; set; }
    }
}
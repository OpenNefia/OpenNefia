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
    public sealed class CookingToolComponent : Component
    {
        /// <summary>
        /// Quality of the cooking tool.
        /// </summary>
        [DataField]
        public int Quality { get; set; } = 0;
    }
}
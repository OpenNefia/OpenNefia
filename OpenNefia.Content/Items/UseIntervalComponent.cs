using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class UseIntervalComponent : Component
    {
        [DataField]
        public GameTimeSpan UseInterval { get; set; }

        [DataField]
        public GameDateTime DateNextUseableOn { get; set; }
    }
}
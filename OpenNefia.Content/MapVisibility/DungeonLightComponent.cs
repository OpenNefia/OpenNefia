using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.MapVisibility
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class DungeonLightComponent : Component
    {
        [DataField]
        public bool IsLit { get; set; }

        [DataField]
        public int LightPower { get; set; } = 50;
    }
}
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
        public override string Name => "DungeonLight";

        [DataField]
        public bool IsLit { get; set; }
    }
}
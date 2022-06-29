using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Home
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapHomeComponent : Component
    {
        public override string Name => "MapHome";

        [DataField]
        public int HomeRankValue { get; set; }
    }
}
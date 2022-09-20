using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Home
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaHomeComponent : Component
    {
        [DataField]
        public int DeedValue { get; set; }

        [DataField]
        public int HomeScale { get; set; }

        [DataField]
        public int HomeRankPoints { get; set; }

        [DataField]
        public int? MaxItemsOnGround { get; set; }
    }
}
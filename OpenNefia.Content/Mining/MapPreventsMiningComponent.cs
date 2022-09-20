using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Mining
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapMiningPreventionComponent : Component
    {
        [DataField]
        public MiningPreventionKind Kind { get; set; } = MiningPreventionKind.AllowMining;
    }

    public enum MiningPreventionKind
    {
        AllowMining,
        NoMinedItems,
        PreventMining
    }
}
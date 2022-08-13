using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Loot
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class RichLootComponent : Component
    {
        public override string Name => "RichLoot";

        [DataField]
        public int RichLootItemCount { get; set; } = 1;
    }
}
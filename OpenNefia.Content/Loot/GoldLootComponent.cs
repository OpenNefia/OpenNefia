using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Loot
{
    /// <summary>
    /// Gold loot dropped by gold bells, based on the player's fame.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class GoldLootComponent : Component
    {
        public override string Name => "GoldLoot";
    }
}
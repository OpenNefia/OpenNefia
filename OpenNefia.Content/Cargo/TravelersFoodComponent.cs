using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Cargo
{
    /// <summary>
    /// This item is automatically consumed by the player if they are traveling in the overworld
    /// while hungry.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TravelersFoodComponent : Component
    {
        public override string Name => "TravelersFood";
    }
}
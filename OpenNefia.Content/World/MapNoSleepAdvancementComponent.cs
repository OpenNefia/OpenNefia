using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.World
{
    /// <summary>
    /// Prevents the player from losing sleep in this map.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapNoSleepAdvancementComponent : Component
    {
    }
}
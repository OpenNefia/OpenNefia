using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Indicates that this map can provide quests.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapQuestHubComponent : Component
    {
    }
}
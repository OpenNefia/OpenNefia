using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must hunt all creatures in an instanced map.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1001</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class QuestTypeHuntComponent : Component
    {
    }
}
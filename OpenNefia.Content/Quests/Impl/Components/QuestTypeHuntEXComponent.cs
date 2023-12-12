using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must hunt a set of powerful creatures of a single type
    /// in a map similar to the originating town map.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1010</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class QuestTypeHuntEXComponent : Component
    {
        [DataField]
        public PrototypeId<EntityPrototype> EnemyID { get; set; } = Protos.Chara.Bug;

        [DataField]
        public int EnemyLevel { get; set; } = 1;
    }
}
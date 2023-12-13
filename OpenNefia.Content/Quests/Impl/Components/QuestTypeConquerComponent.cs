using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must hunt a single god-tier creature
    /// in a map similar to the originating town map.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1008</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeConquerComponent : Component
    {
        [DataField]
        public PrototypeId<EntityPrototype> EnemyID { get; set; } = Protos.Chara.Bug;

        [DataField]
        public int EnemyLevel { get; set; } = 1;

        [DataField]
        public EntityUid TargetEnemyUid { get; set; } = EntityUid.Invalid;
    }

    /// <summary>
    /// Indicates this entity is the conquer quest target.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ConquerQuestTargetComponent : Component
    {
        public EntityUid QuestUid { get; set; } = EntityUid.Invalid;
    }
}
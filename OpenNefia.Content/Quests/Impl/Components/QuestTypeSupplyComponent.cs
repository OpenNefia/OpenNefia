using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace OpenNefia.Content.Quests.Impl
{
    /// <summary>
    /// Quest in which the player must give the client a specific item.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1004</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeSupplyComponent : Component
    {
        /// <summary>
        /// Target items that this quest can be generated with.
        /// </summary>
        [DataField]
        public PrototypeId<TagSetPrototype> TargetItemCandidates { get; set; } = Protos.TagSet.ItemSupply;

        /// <summary>
        /// ID of the item the client desires.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> TargetItemID { get; set; } = Protos.Item.Bug;
    }
}
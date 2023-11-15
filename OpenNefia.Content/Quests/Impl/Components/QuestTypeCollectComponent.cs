using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must give the client a specific item, which
    /// is generated in another NPC's inventory.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1011</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeCollectComponent : Component
    {
        /// <summary>
        /// Character holding the target item (should always be in the same map as the client).
        /// </summary>
        [DataField]
        public EntityUid TargetChara { get; set; }

        /// <summary>
        /// Name of the target character.
        /// </summary>
        [DataField]
        public string? TargetCharaName { get; set; }

        /// <summary>
        /// ID of the item the client desires.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> TargetItemID { get; set; } = Protos.Item.Bug;
    }
}
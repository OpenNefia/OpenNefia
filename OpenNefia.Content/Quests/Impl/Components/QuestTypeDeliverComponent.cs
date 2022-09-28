using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Data;

namespace OpenNefia.Content.Quests
{
    /// <summary>
    /// Quest in which the player must give the client a specific item.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1002</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeDeliverComponent : Component
    {
        /// <summary>
        /// Character to deliver the item to (should always be in a different map than the client).
        /// </summary>
        [DataField]
        public EntityUid TargetChara { get; set; }

        /// <summary>
        /// Name of the target character.
        /// </summary>
        [DataField]
        public string TargetCharaName { get; set; } = string.Empty;

        /// <summary>
        /// Map containing the target character.
        /// </summary>
        [DataField]
        public MapId TargetMapID { get; set; }

        /// <summary>
        /// Name of the map containing the target character.
        /// </summary>
        [DataField]
        public string TargetMapName { get; set; } = string.Empty;

        /// <summary>
        /// Category of the item the client desires.
        /// </summary>
        [DataField]
        public PrototypeId<TagPrototype> TargetItemCategory { get; set; } = Protos.Tag.ItemCatBug;

        /// <summary>
        /// ID of the item the client desires.
        /// </summary>
        [DataField]
        public PrototypeId<EntityPrototype> TargetItemID { get; set; } = Protos.Item.Bug;
    }
}
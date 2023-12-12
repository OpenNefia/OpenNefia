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
    /// Quest in which the player must escort the client to another town in the specified time.
    /// </summary>
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1007</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Quest)]
    public sealed class QuestTypeEscortComponent : Component
    {
        /// <summary>
        /// Variation of the escort quest to run.
        /// </summary>
        [DataField]
        public EscortType EscortType { get; set; } = EscortType.Protect;

        /// <summary>
        /// Map to escort to.
        /// </summary>
        [DataField]
        public MapId TargetMapID { get; set; }

        /// <summary>
        /// Name of map to escort to.
        /// </summary>
        [DataField]
        public string TargetMapName { get; set; } = string.Empty;

        /// <summary>
        /// Entity UID being escorted.
        /// </summary>
        [DataField]
        public EntityUid EscortingChara { get; set; }

        /// <summary>
        /// Number of random world map encounters triggered for this quest so far.
        /// </summary>
        [DataField]
        public int EncountersSeen { get; set; }
    }

    public enum EscortType
    {
        Protect,
        Poison,
        Deadline
    }

    /// <summary>
    /// Indicates this character is a quest escort.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EscortedInQuestComponent : Component
    {
        [DataField]
        public EntityUid QuestUid { get; set; }
    }
}
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Defines a Nefia area.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaNefiaComponent : Component
    {
        /// <summary>
        /// Current state of this Nefia.
        /// </summary>
        [DataField]
        public NefiaState State { get; set; }

        /// <summary>
        /// UID of the generated Nefia boss.
        /// </summary>
        [DataField]
        public EntityUid? BossEntityUid { get; set; }

        /// <summary>
        /// Base level of this Nefia.
        /// </summary>
        [DataField]
        public int BaseLevel { get; internal set; }

        /// <summary>
        /// Used for getting the display name of each Nefia floor.
        /// </summary>
        /// <remarks>
        /// Typically "Elona.Nefia.NameModifiers.TypeA" or "Elona.Nefia.NameModifiers.TypeB".
        /// </remarks>
        [DataField]
        public LocaleKey NameType { get; set; }

        /// <summary>
        /// Used for getting the display name of each Nefia floor.
        /// </summary>
        /// <remarks>
        /// Typically between 0-4.
        /// </remarks>
        [DataField]
        public int NameRank { get; set; }
    }

    /// <summary>
    /// Possible states of the Nefia.
    /// </summary>
    public enum NefiaState
    {
        /// <summary>
        /// The player has not visited this Nefia yet.
        /// </summary>
        Unvisited,

        /// <summary>
        /// The player has visited this Nefia before.
        /// </summary>
        Visited,

        /// <summary>
        /// The player has successfully defeated the Nefia boss on the final
        /// floor.
        /// </summary>
        /// <remarks>
        /// The area is counted as "inactive" in this state.
        /// </remarks>
        Conquered,

        /// <summary>
        /// The player left the final floor while the Nefia boss was there,
        /// but did not defeat them first.
        /// </summary>
        /// <remarks>
        /// The area is counted as "inactive" in this state.
        /// </remarks>
        BossVanished
    }
}

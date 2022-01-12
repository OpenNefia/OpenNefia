using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
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
        /// <inheritdoc />
        public override string Name => "AreaNefia";

        /// <summary>
        /// Current state of this Nefia.
        /// </summary>
        public NefiaState State { get; set; }

        /// <summary>
        /// UID of the generated Nefia boss.
        /// </summary>
        public EntityUid? BossEntityUid { get; set; }

        /// <summary>
        /// Base level of this Nefia.
        /// </summary>
        public int BaseLevel { get; internal set; }
    }

    /// <summary>
    /// Possible states of a Nefia.
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

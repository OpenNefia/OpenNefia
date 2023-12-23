using OpenNefia.Content.Areas;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Return
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public sealed class AreaReturnDestinationComponent : Component
    {
        /// <summary>
        /// If true, the player can Return to this area if it's been visited before.
        /// </summary>
        /// <remarks>
        /// This is a property of the area, not a flag, so it should be set in prototypes.
        /// </remarks>
        [DataField]
        public bool CanBeReturnDestination { get; set; } = false;

        /// <summary>
        /// If true, at least one map within this area has been visited before,
        /// and the area can be considered as a Return destination.
        /// </summary>
        [DataField]
        public bool HasEverBeenVisited { get; set; } = false;

        /// <summary>
        /// Deepest floor the player has visited in this area. Used when
        /// determining the area floor to return to.
        /// </summary>
        /// <remarks>
        /// If the area has a <see cref="AreaTypeTownComponent"/>, this is ignored,
        /// and the floor returned to will be the town's starting floor.
        /// </remarks>
        [DataField]
        public int DeepestFloorVisited { get; set; } = 0;

        /// <summary>
        /// Area floor to return to. Overrides the other return floor
        /// calculation logic.
        /// </summary>
        /// <remarks>
        /// Also meant to be a property of the map, and shouldn't be updated
        /// in code.
        /// </remarks>
        [DataField]
        public AreaFloorId? ReturnFloor { get; set; } = null;
    }
}
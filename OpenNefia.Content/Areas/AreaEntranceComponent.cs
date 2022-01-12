using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Areas
{
    /// <summary>
    /// Specifies defaults for generated <see cref="WorldMapEntranceComponent"/>s that
    /// lead to this area. Also allows setting a global area ID for this area..
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Area)]
    public class AreaEntranceComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "AreaEntrance";

        /// <summary>
        /// If non-null, create a global area with this ID when 
        /// starting a new save.
        /// </summary>
        [DataField]
        public GlobalAreaId? GlobalId { get; set; }

        /// <summary>
        /// Starting floor of this area.
        /// </summary>
        [DataField]
        public AreaFloorId? StartingFloor { get; set; }

        /// <summary>
        /// Position to place the player on when entering the starting floor. 
        /// This is copied to the generated <see cref="WorldMapEntranceComponent"/>.
        /// </summary>
        [DataField]
        public IMapStartLocation? StartLocation { get; set; }

        /// <summary>
        /// Extra components to spawn the map entrance entity with.
        /// </summary>
        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}

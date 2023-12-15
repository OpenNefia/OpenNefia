using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Rendering
{
    /// <summary>
    /// Entities with this component can "scroll" around the map
    /// whenever they move.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ScrollableComponent : Component
    {
        [DataField]
        public MapCoordinates PreviousMapPosition { get; set; }
    }
}
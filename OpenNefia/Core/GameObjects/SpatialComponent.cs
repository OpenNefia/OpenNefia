using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Contains information about the entity's position in a map.
    /// </summary>
    [RegisterComponent]
    public class SpatialComponent : Component
    {
        public override string Name => "Spatial";

        /// <summary>
        /// If true, this entity cannot be moved over. This also causes
        /// collision events to be fired when the entity is moved into.
        /// </summary>
        [DataField]
        public bool IsSolid { get; set; }

        /// <summary>
        /// If true, this entity blocks field of view.
        /// </summary>
        [DataField]
        public bool IsOpaque { get; set; }
    }
}

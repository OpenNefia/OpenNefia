using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Logic for retrieving the map that a <see cref="MapEntrance"/> should
    /// use as its destination.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IMapIdSpecifier
    {
        /// <summary>
        /// Gets the area relevant to this destination, if any.
        /// </summary>
        public AreaId? GetAreaId();

        /// <summary>
        /// Gets the map to be used for the destination.
        /// </summary>
        /// <returns>The map ID, or null if something failed.</returns>
        public MapId? GetMapId();
    }
}

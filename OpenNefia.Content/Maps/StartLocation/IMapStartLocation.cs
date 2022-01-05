using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// A location on a map to place the player/allies when
    /// travelling to the map.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IMapStartLocation
    {
        /// <summary>
        /// Returns the position to place this entity on the map.
        /// </summary>
        /// <remarks>
        /// If the position is inaccessible, the intention is for
        /// the calling code to find an open space near the returned
        /// position.
        /// </remarks>
        /// <param name="ent">Entity travelling, typically the player or allies.</param>
        /// <param name="map">Map travelling to.</param>
        /// <returns>A desired position to place the entity.</returns>
        public Vector2i GetStartPosition(EntityUid ent, IMap map);
    }
}

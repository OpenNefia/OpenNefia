using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
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

    /// <summary>
    /// Places the player at the center of the map.
    /// </summary>
    /// <remarks>
    /// According to <see cref="MapComponent"/>, this is the default.
    /// </remarks>
    public class CenterMapLocation : IMapStartLocation
    {
        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            return map.Size / 2;
        }
    }

    /// <summary>
    /// Places the player at a specific location in the map.
    /// </summary>
    public class SpecificMapLocation : IMapStartLocation
    {
        [DataField("pos")]
        public Vector2i Position { get; }

        public SpecificMapLocation() { }

        public SpecificMapLocation(Vector2i position)
        {
            Position = position;
        }

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            return Position.BoundWithin(map.Bounds);
        }
    }
}

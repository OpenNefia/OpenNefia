using OpenNefia.Core.GameObjects;
using OpenNefia.Core.GameObjects.Components;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
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
            return Position;
        }
    }

    /// <summary>
    /// Places the player at the edge of a map, based on what direction they're
    /// facing in the world map.
    /// </summary>
    public class MapEdgesLocation : IMapStartLocation, ISerializationHooks
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            EntitySystem.InjectDependencies(this);

            if (!_entityManager.TryGetComponent(ent, out SpatialComponent? spatial))
                return new CenterMapLocation().GetStartPosition(ent, map);

            var targetPos = GetDefaultTargetCoords(map, spatial.Direction);

            foreach (var comp in _entityLookup.EntityQueryInMap<MapEdgesLocationComponent>(map.Id))
            {
                if (comp.TargetDirection == spatial.Direction)
                {
                    targetPos = _entityManager.GetComponent<SpatialComponent>(comp.OwnerUid).WorldPosition;
                    break;
                }
            }

            return targetPos;
        }

        private Vector2i GetDefaultTargetCoords(IMap map, Direction direction)
        {
            switch (direction)
            {
                case Direction.North:
                    return new Vector2i(map.Width / 2, map.Height - 2);
                case Direction.South:
                    return new Vector2i(map.Width / 2, 1);
                case Direction.East:
                    return new Vector2i(1, map.Height / 2);
                case Direction.West:
                    return new Vector2i(map.Width - 2, map.Height / 2);
                default:
                    return map.Size / 2;
            }
        }
    }
}

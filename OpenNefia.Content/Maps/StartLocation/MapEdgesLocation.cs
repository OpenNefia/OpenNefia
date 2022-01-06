using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
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
                    targetPos = _entityManager.GetComponent<SpatialComponent>(comp.Owner).WorldPosition;
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
                case Direction.NorthWest:
                case Direction.NorthEast:
                    return new Vector2i(map.Width / 2, map.Height - 2);
                case Direction.South:
                case Direction.SouthWest:
                case Direction.SouthEast:
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

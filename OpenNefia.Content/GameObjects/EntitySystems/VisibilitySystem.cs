using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public interface IVisibilitySystem : IEntitySystem
    {
        /// <summary>
        /// Returns true if the entity has line of sight to the position of the target.
        /// Ignores invisibility checks.
        /// </summary>
        bool HasLineOfSight(EntityUid onlooker, EntityUid target,
            SpatialComponent? onlookerSpatial = null,
            SpatialComponent? targetSpatial = null);

        /// <summary>
        /// Returns true if the entity has line of sight to the map position.
        /// </summary>
        bool HasLineOfSight(EntityUid onlooker, MapCoordinates targetPos,
            SpatialComponent? spatial = null);

        /// <summary>
        /// Returns true if the onlooker can see the entity, including visibility checks.
        /// </summary>
        bool CanSeeEntity(EntityUid onlooker, EntityUid target);
    }

    public class VisibilitySystem : EntitySystem, IVisibilitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public bool HasLineOfSight(EntityUid onlooker, EntityUid target,
            SpatialComponent? onlookerSpatial = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(onlooker, ref onlookerSpatial) || !Resolve(target, ref targetSpatial))
                return false;

            return HasLineOfSight(onlooker, targetSpatial.MapPosition, onlookerSpatial);
        }

        public bool HasLineOfSight(EntityUid onlooker, MapCoordinates targetPos,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(onlooker, ref spatial))
                return false;

            if (targetPos.MapId != spatial.MapID)
                return false;

            if (!_mapManager.TryGetMap(targetPos.MapId, out var map))
                return false;

            if (!map.HasLineOfSight(spatial.WorldPosition, targetPos.Position))
                return false;

            // The player can't see positions that aren't in the game window
            // (same as vanilla).
            if (_gameSession.IsPlayer(onlooker) && !map.IsInWindowFov(targetPos.Position))
                return false;

            return true;
        }

        public bool CanSeeEntity(EntityUid onlooker, EntityUid target)
        {
            if (!EntityManager.TryGetComponent(target, out SpatialComponent targetSpatial))
                return false;

            if (EntityManager.TryGetComponent(target, out MapComponent? map)
                || EntityManager.TryGetComponent(onlooker, out map))
            {
                return _mapManager.ActiveMap?.Id == map.MapId;
            }

            // TODO

            return HasLineOfSight(onlooker, targetSpatial.MapPosition);
        }
    }
}
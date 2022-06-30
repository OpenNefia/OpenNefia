using Love;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Visibility
{
    public interface IVisibilitySystem : IEntitySystem
    {
        /// <summary>
        /// Returns true if the tile this entity is on is within the visible bounds of the
        /// game window. Does not do any invisibility checks.
        /// </summary>
        bool IsInWindowFov(EntityUid target, SpatialComponent? spatial = null);

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

        public override void Initialize()
        {
            SubscribeComponent<VisibilityComponent, EntityRefreshEvent>(OnEntityRefresh, priority: EventPriorities.Highest);
        }

        private void OnEntityRefresh(EntityUid uid, VisibilityComponent vis, ref EntityRefreshEvent args)
        {
            vis.IsInvisible.Reset();
            vis.CanSeeInvisible.Reset();
        }

        public bool IsInWindowFov(EntityUid target, SpatialComponent? spatial = null)
        {
            if (!Resolve(target, ref spatial))
                return false;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return false;

            return map.IsInWindowFov(spatial.WorldPosition);
        }

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

            if (TryComp<VisibilityComponent>(target, out var vis) && vis.IsInvisible.Buffed)
            {
                if (!TryComp<VisibilityComponent>(onlooker, out var onlookerVis) || !onlookerVis.CanSeeInvisible.Buffed)
                    return false;
            }

            return HasLineOfSight(onlooker, targetSpatial.MapPosition);
        }
    }
}
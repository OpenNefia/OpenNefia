using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class VisibilitySystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public bool CanSeeEntity(EntityUid target, 
            EntityUid? onlooker = null)
        {
            if (!EntityManager.TryGetComponent(target, out SpatialComponent spatial))
                return false;

            if (onlooker == null)
                return true;

            return CanSeePosition(target, spatial.MapPosition);
        }

        public bool CanSeePosition(EntityUid onlooker, MapCoordinates targetPos,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(onlooker, ref spatial))
                return false;

            if (targetPos.MapId != spatial.MapID)
                return false;

            if (!_mapManager.TryGetMap(targetPos.MapId, out var map))
                return false;

            if (!map.HasLos(spatial.WorldPosition, targetPos.Position))
                return false;

            // The player can't see positions that aren't in the game window
            // (same as vanilla).
            if (EntityManager.HasComponent<PlayerComponent>(onlooker) && !map.IsInWindowFov(targetPos.Position))
                return false;

            return true;
        }
    }
}
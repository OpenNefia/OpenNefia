using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class VisibilitySystem : EntitySystem
    {
        public bool CanSeeEntity(EntityUid target, 
            EntityUid? onlooker = null)
        {
            if (!EntityManager.TryGetComponent(target, out SpatialComponent spatial))
                return false;

            if (onlooker == null)
                return true;

            return CanSeePosition(target, spatial.Coords);
        }

        public bool CanSeePosition(EntityUid onlooker, MapCoordinates targetPos,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(onlooker, ref spatial))
                return false;

            if (!spatial.Coords.HasLos(targetPos))
                return false;

            // The player can't see positions that aren't in the game window
            // (same as vanilla).
            if (EntityManager.HasComponent<PlayerComponent>(onlooker) 
                && !targetPos.IsInWindowFov())
                return false;

            return true;
        }
    }
}
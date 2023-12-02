using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Visibility
{
    public interface IVisibilitySystem : IEntitySystem
    {
        /// <summary>
        /// Returns true if the tile this entity is on is within the visible bounds of the
        /// game window. Does not do any invisibility checks.
        /// </summary>
        bool IsInWindowFov(EntityUid target, SpatialComponent? spatial = null);
        bool IsInWindowFov(MapCoordinates coords);
        bool IsInWindowFov(EntityCoordinates coords);

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
        bool CanSeeEntity(EntityUid onlooker, EntityUid target, bool ignoreLos = false);

        bool TryToPercieve(EntityUid perceiver, EntityUid target);
    }

    public class VisibilitySystem : EntitySystem, IVisibilitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;

        public override void Initialize()
        {
            SubscribeComponent<VisibilityComponent, MapBeforeTurnBeginEventArgs>(BeforeTurnBegin);
            SubscribeComponent<VisibilityComponent, EntityTurnStartingEventArgs>(OnTurnStart);
        }

        private void BeforeTurnBegin(EntityUid uid, VisibilityComponent component, MapBeforeTurnBeginEventArgs args)
        {
            if (args.Handled)
                return;

            if (TryComp<VanillaAIComponent>(uid, out var vai))
            {
                var target = vai.CurrentTarget;
                if (EntityManager.IsAlive(target) && !CanSeeEntity(uid, target.Value))
                    vai.CurrentTarget = null;
            }
        }

        private void OnTurnStart(EntityUid uid, VisibilityComponent vis, EntityTurnStartingEventArgs args)
        {
            vis.Noise = 0;
        }

        public bool IsInWindowFov(EntityUid target, SpatialComponent? spatial = null)
        {
            if (!Resolve(target, ref spatial))
                return false;

            return IsInWindowFov(spatial.MapPosition);
        }

        public bool IsInWindowFov(MapCoordinates coords)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return false;

            return map.IsInWindowFov(coords.Position);
        }

        public bool IsInWindowFov(EntityCoordinates coords)
        {
            return IsInWindowFov(coords.ToMap(EntityManager));
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

        public bool CanSeeEntity(EntityUid onlooker, EntityUid target, bool ignoreLos = false)
        {
            if (!EntityManager.TryGetComponent(target, out SpatialComponent targetSpatial))
                return false;

            if (!TryMap(target, out var map) || _mapManager.ActiveMap?.Id != map.Id)
                return false;

            // Making sure the player can always "see" the map entity is
            // important, because localization functions depend on visibility.
            // Without this check, any call to `_.name(mapEntityUid)` in a
            // locale file will just return "something".
            if (EntityManager.TryGetComponent(target, out MapComponent? mapComp)
                || EntityManager.TryGetComponent(onlooker, out mapComp))
            {
                return _mapManager.ActiveMap?.Id == mapComp.MapId;
            }

            if (TryComp<VisibilityComponent>(target, out var vis) && vis.IsInvisible.Buffed)
            {
                if (!TryComp<VisibilityComponent>(onlooker, out var onlookerVis) || !onlookerVis.CanSeeInvisible.Buffed)
                    return false;
            }

            return ignoreLos || HasLineOfSight(onlooker, targetSpatial.MapPosition);
        }

        private bool IsInSquare(MapCoordinates from, MapCoordinates to, int radius)
        {
            if (from.MapId != to.MapId)
                return false;

            var fp = from.Position;
            var tp = to.Position;
            return fp.X > tp.X - radius && fp.X < tp.X + radius && fp.Y > tp.Y - radius && fp.Y < tp.Y + radius;
        }

        public bool TryToPercieve(EntityUid perceiver, EntityUid target)
        {
            // >>>>>>>> shade2/calculation.hsp:1141 *calcStealth ...
            var radius = 8;
            var perceiverPos = Spatial(perceiver).MapPosition;
            var targetPos = Spatial(target).MapPosition;
            if (IsInSquare(perceiverPos, targetPos, radius))
            {
                if (TryComp<VanillaAIComponent>(perceiver, out var ai) && ai.Aggro > 0)
                    return true;

                if (perceiverPos.TryDistanceTiled(targetPos, out var dist))
                {
                    var chance = dist * 150 + _skills.Level(target, Protos.Skill.Stealth) + 100 + 150 + 1;
                    if (_rand.Next(chance) < _rand.Next(_skills.Level(perceiver, Protos.Skill.AttrPerception) * 60 + 150))
                        return true;
                }
            }

            if (TryComp<VisibilityComponent>(target, out var vis) && vis.Noise > 0 && _rand.Next(150) < vis.Noise)
                return true;

            return false;
            // <<<<<<<< shade2/calculation.hsp:1149 	return false ..
        }
    }
}
using ICSharpCode.Decompiler.Util;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public interface ITargetingSystem : IEntitySystem
    {
        bool PromptReallyAttack(EntityUid player, EntityUid target);

        List<SpatialComponent> FindTargets(EntityUid attacker);

        /// <hsp>*findTarget</hsp>
        bool TryGetTarget(EntityUid attacker, [NotNullWhen(true)] out EntityUid? target, bool silent = false);
    }

    public sealed class TargetingSystem : EntitySystem, ITargetingSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ITargetTextSystem _targetText = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAI = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public bool PromptReallyAttack(EntityUid player, EntityUid target)
        {
            if (!_gameSession.IsPlayer(player))
                return true;

            _mes.Display(_targetText.GetTargetDangerText(player, target));
            return _playerQuery.YesOrNo(Loc.GetString("Elona.Targeting.PromptReallyAttack", ("target", target)));
        }

        public List<SpatialComponent> FindTargets(EntityUid attacker)
        {
            // >>>>>>>> shade2/command.hsp:251 		rc=-1 ...
            if (!TryMap(attacker, out var map))
                return new List<SpatialComponent>();

            var attackerSpatial = Spatial(attacker);

            bool IsValidTarget(SpatialComponent otherSpatial)
            {
                var other = otherSpatial.Owner;

                if (attacker == other || !_vis.IsInWindowFov(other))
                    return false;

                if (_parties.TryGetLeader(attacker, out var leader) && other == leader.Value)
                    return false;

                if (!_vis.HasLineOfSight(attacker, other))
                    return false;

                if (!_vis.CanSeeEntity(attacker, other))
                    return false;

                return true;
            }

            // Sort by closeness to the targeting character.
            int Sort(SpatialComponent a, SpatialComponent b)
            {
                if (!attackerSpatial!.MapPosition.TryDistanceFractional(a.MapPosition, out var distA))
                    distA = int.MaxValue;
                if (!attackerSpatial!.MapPosition.TryDistanceFractional(a.MapPosition, out var distB))
                    distB = int.MaxValue;
                return distA.CompareTo(distB);
            }

            var targets = _lookup.EntityQueryInMap<SpatialComponent>(map)
                .Where(IsValidTarget)
                .ToList();

            targets.Sort(Sort);

            return targets;
        }

        /// <inheritdoc/>
        public bool TryGetTarget(EntityUid attacker, [NotNullWhen(true)] out EntityUid? target, bool silent = false)
        {
            // >>>>>>>> shade2/command.hsp:4240 *findTarget ...
            if (!TryComp<VanillaAIComponent>(attacker, out var vai))
            {
                target = null;
                return false;
            }

            if (!IsAlive(vai.CurrentTarget) || !_vis.IsInWindowFov(vai.CurrentTarget.Value))
            {
                _vanillaAI.SetTarget(attacker, null, ai: vai);
            }

            target = _vanillaAI.GetTarget(attacker, ai: vai);
            if (!IsAlive(target))
            {
                var targetSpatial = FindTargets(attacker)
                    .Where(spatial => _factions.GetRelationTowards(attacker, spatial.Owner) <= Relation.Enemy)
                    .FirstOrDefault();

                if (targetSpatial != null)
                    _vanillaAI.SetTarget(attacker, targetSpatial.Owner, ai: vai);
            }

            target = _vanillaAI.GetTarget(attacker, ai: vai);
            if (!IsAlive(target) || _effects.HasEffect(attacker, Protos.StatusEffect.Blindness))
            {
                if (_gameSession.IsPlayer(attacker) && !silent)
                    _mes.Display(Loc.GetString("Elona.Targeting.NoTarget"));
                
                target = null;
                return false;
            }

            return true;
            // <<<<<<<< shade2/command.hsp:4264 	return true ..
        }
    }
}
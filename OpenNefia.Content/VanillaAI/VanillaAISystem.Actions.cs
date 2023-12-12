using CSharpRepl.Services.Theming;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Factions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    public sealed partial class VanillaAISystem
    {
        [Dependency] private readonly ICombatSystem _combat = default!;

        public const int AIRangedAttackThreshold = 6;

        private void DoBasicAction(EntityUid entity, VanillaAIComponent ai)
        {
            // >>>>>>>> shade2/ai.hsp:470 *ai_actMain ..
            if (!IsAlive(ai.CurrentTarget))
                return;

            var target = ai.CurrentTarget.Value;
            if (_parties.TryGetLeader(target, out var leader))
            {
                target = leader.Value;
                if (TryComp<VanillaAIComponent>(leader.Value, out var leaderAI))
                    leaderAI.LastAttacker = entity;
            }

            // TODO
            DoActionPhysicalAttack(entity, target, ai);
            // <<<<<<<< shade2/ai.hsp:527 	 ..
        }

        private TurnResult? DoActionPhysicalAttack(EntityUid attacker, EntityUid target,
            VanillaAIComponent? ai = null,
            SpatialComponent? spatial = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(attacker, ref spatial, ref ai))
                return TurnResult.Failed;

            if (!EntityManager.IsAlive(target) || !Resolve(target, ref targetSpatial))
                return TurnResult.Failed;

            if (!spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                return TurnResult.Failed;

            if (dist <= 1)
            {
                return AttemptToMelee(attacker, targetSpatial.Owner);
            }

            if (dist < AIRangedAttackThreshold && _vis.HasLineOfSight(attacker, target))
            {
                if (_combat.TryGetRangedWeaponAndAmmo(attacker, out var rangedWeapon, out _))
                {
                    return _combat.RangedAttack(attacker, target, rangedWeapon.Value);
                }
            }

            if (ai.TargetDistance <= dist)
            {
                if (_rand.OneIn(3))
                {
                    return TurnResult.Succeeded;
                }
            }

            if (_rand.OneIn(5))
            {
                DecrementAggro(ai);
            }

            if (_rand.Prob(ai.MoveChance))
            {
                DoIdleAction(attacker, ai);
                return TurnResult.Succeeded;
            }

            return TurnResult.Failed;
        }

        private TurnResult? AttemptToMelee(EntityUid attacker, EntityUid target,
            SpatialComponent? spatial = null,
            VanillaAIComponent? ai = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(attacker, ref spatial, ref ai))
                return TurnResult.Failed;

            if (!EntityManager.IsAlive(target) || !Resolve(target, ref targetSpatial))
                return TurnResult.Failed;

            if (!spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                return TurnResult.Failed;

            if (dist <= 1)
            {
                return _combat.MeleeAttack(attacker, target) ?? TurnResult.Failed;
            }

            return TurnResult.Failed;
        }

        private TurnResult? DoActionRangedAttack(EntityUid attacker, EntityUid target,
            VanillaAIComponent? ai = null,
            SpatialComponent? spatial = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(attacker, ref spatial, ref ai))
                return TurnResult.Failed;

            if (!EntityManager.IsAlive(target) || !Resolve(target, ref targetSpatial))
                return TurnResult.Failed;

            if (!spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                return TurnResult.Failed;

            if (dist < AIRangedAttackThreshold && _vis.HasLineOfSight(attacker, target))
            {
                if (_combat.TryGetRangedWeaponAndAmmo(attacker, out var rangedWeapon, out _))
                {
                    return _combat.RangedAttack(attacker, target, rangedWeapon.Value);
                }
            }

            return TurnResult.Failed;
        }
    }

    public class RunAIActionEvent : TurnResultEntityEventArgs
    {
        public EntityUid Actor { get; }

        public RunAIActionEvent(EntityUid actor)
        {
            Actor = actor;
        }
    }
}

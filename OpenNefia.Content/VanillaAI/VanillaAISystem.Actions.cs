using OpenNefia.Content.GameObjects;
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
        [Dependency] private readonly CombatSystem _combat = default!;

        private void SubscribeAIActions()
        {
            SubscribeLocalEvent<AIActionMeleeComponent, RunAIActionEvent>(HandleActionMelee);
        }

        private void HandleActionMelee(EntityUid action, AIActionMeleeComponent component, RunAIActionEvent args)
        {
            if (args.Handled)
                return;

            args.Handle(DoActionMelee(args.Actor, component));
        }

        private TurnResult DoActionMelee(EntityUid attacker, AIActionMeleeComponent actMelee,
            SpatialComponent? spatial = null,
            VanillaAIComponent? ai = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(attacker, ref spatial, ref ai))
                return TurnResult.Failed;

            if (!EntityManager.IsAlive(ai.CurrentTarget) || !Resolve(ai.CurrentTarget.Value, ref targetSpatial))
                return TurnResult.Failed;

            if (!spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                return TurnResult.Failed;

            if (dist <= 1)
            {
                return AttemptToMelee(attacker, targetSpatial.Owner);
            }
            
            // TODO ranged

            if (ai.TargetDistance <= dist)
            {
                if (_random.OneIn(3))
                {
                    return TurnResult.Succeeded;
                }
            }

            if (_random.OneIn(5))
            {
                DecrementAggro(ai);
            }

            if (_random.Prob(ai.MoveChance))
            {
                DoIdleAction(attacker, ai);
                return TurnResult.Succeeded;
            }

            return TurnResult.Failed;
        }

        private TurnResult AttemptToMelee(EntityUid attacker, EntityUid target,
            SpatialComponent? spatial = null,
            VanillaAIComponent? ai = null,
            SpatialComponent? targetSpatial = null)
        {
            if (!Resolve(attacker, ref spatial, ref ai))
                return TurnResult.Failed;

            if (!EntityManager.IsAlive(ai.CurrentTarget) || !Resolve(ai.CurrentTarget.Value, ref targetSpatial))
                return TurnResult.Failed;

            if (!spatial.MapPosition.TryDistanceTiled(targetSpatial.MapPosition, out var dist))
                return TurnResult.Failed;

            if (dist <= 1)
            {
                return _combat.MeleeAttack(attacker, target) ?? TurnResult.Failed;
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

using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Maths;
using OpenNefia.Content.VanillaAI;
using Love;
using OpenNefia.Core.Utility;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Combat;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Actions;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.VisualAI.Block
{
    [ImplicitDataDefinitionForInheritors]
    public interface IVisualAIAction
    {
        bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target);
    }


    public abstract class BaseAction : IVisualAIAction
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        public abstract bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target);
    }

    public sealed class DoNothingAction : BaseAction
    {
        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            return true;
        }
    }

    public sealed class WanderAction : BaseAction
    {
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IVanillaAISystem _vai = default!;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            _vai.Wander(state.AIEntity);
            return true;
        }
    }

    public sealed class MoveWithinDistanceAction : BaseAction
    {
        [Dependency] private readonly IMapManager _mapMan = default!;
        [Dependency] private readonly IVanillaAISystem _vai = default!;

        [DataField]
        [VisualAIVariable(minValue: 0)]
        public int Threshold { get; set; } = 0;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            var spatial = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var map = state.Map;

            var minDist = Threshold;
            if (!map.CanAccess(target.Coordinates))
                minDist = int.Max(Threshold, 1);

            if (!spatial.MapPosition.TryDistanceFractional(target.Coordinates, out var dist)
                || dist <= minDist)
            {
                return false;
            }

            if (EntityManager.IsAlive(target.Entity))
            {
                if (state.AIEntity == target.Entity.Value)
                    return false;
                return _vai.MoveTowardsTarget(state.AIEntity, target.Entity);
            }
            else
            {
                return _vai.StayNearPosition(state.AIEntity, target.Coordinates);
            }
        }
    }

    public sealed class RetreatFurthestAction : BaseAction
    {
        [Dependency] private readonly IVanillaAISystem _vai = default!;
        [Dependency] private readonly IMoveableSystem _moveables = default!;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            var spatial = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);

            if (EntityManager.IsAlive(target.Entity))
            {
                if (state.AIEntity == target.Entity.Value)
                    return false;

                return _vai.MoveTowardsTarget(state.AIEntity, target.Entity, retreat: true);
            }
            else
            {
                if (!DirectionUtility.TryDirectionTowards(spatial.MapPosition, target.Coordinates, out var dir))
                    return false;

                var newPos = new MapCoordinates(spatial.MapPosition.MapId, spatial.MapPosition.Position - dir.ToIntVec());

                var map = state.Map;

                if (map.CanAccess(newPos))
                {
                    _moveables.MoveEntity(state.AIEntity, newPos);
                    return true;
                }

                return false;
            }
        }
    }

    public sealed class RetreatUntilDistanceAction : BaseAction
    {
        [Dependency] private readonly IVanillaAISystem _vai = default!;
        [Dependency] private readonly IMoveableSystem _moveables = default!;

        [DataField]
        [VisualAIVariable(minValue: 0)]
        public int Threshold { get; set; } = 3;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            var spatial = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var map = state.Map;

            if (!spatial.MapPosition.TryDistanceFractional(target.Coordinates, out var dist)
                || dist >= Threshold)
            {
                return false;
            }

            if (EntityManager.IsAlive(target.Entity))
            {
                if (state.AIEntity == target.Entity.Value)
                    return false;

                return _vai.MoveTowardsTarget(state.AIEntity, target.Entity, retreat: true);
            }
            else
            {
                if (!DirectionUtility.TryDirectionTowards(spatial.MapPosition, target.Coordinates, out var dir))
                    return false;

                var newPos = new MapCoordinates(spatial.MapPosition.MapId, spatial.MapPosition.Position - dir.ToIntVec());

                if (map.CanAccess(newPos))
                {
                    _moveables.MoveEntity(state.AIEntity, newPos);
                    return true;
                }

                return false;
            }
        }
    }

    public sealed class MeleeAttackAction : BaseAction
    {
        [Dependency] private readonly ICombatSystem _combat = default!;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            if (!EntityManager.IsAlive(target.Entity))
                return false;

            var spatial = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);

            if (!spatial.MapPosition.TryDistanceTiled(target.Coordinates, out var dist)
                || dist > 1)
            {
                return false;
            }

            return _combat.MeleeAttack(state.AIEntity, target.Entity.Value) == TurnResult.Succeeded;
        }
    }

    public sealed class RangedAttackAction : BaseAction
    {
        [Dependency] private readonly ICombatSystem _combat = default!;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            if (!EntityManager.IsAlive(target.Entity))
                return false;

            if (!_combat.TryGetRangedWeaponAndAmmo(state.AIEntity, out var rangedWeapon, out _))
                return false;

            var spatial = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);

            if (!spatial.MapPosition.TryDistanceTiled(target.Coordinates, out var dist)
                || dist >= VanillaAISystem.AIRangedAttackThreshold)
            {
                return false;
            }

            return _combat.RangedAttack(state.AIEntity, target.Entity.Value, rangedWeapon.Value) == TurnResult.Succeeded;
        }
    }

    public sealed class CastSpellAction : BaseAction
    {
        [Dependency] private readonly IVanillaAISystem _vai = default!;
        [Dependency] private readonly ISpellSystem _spells = default!;

        [DataField]
        [VisualAIVariable]
        public PrototypeId<SpellPrototype> SpellID { get; set; } = Protos.Spell.MagicDart;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            if (!EntityManager.IsAlive(target.Entity))
                return false;

            if (!_spells.HasSpell(state.AIEntity, SpellID))
                return false;

            _vai.SetTarget(state.AIEntity, target.Entity.Value);
            return _spells.Cast(state.AIEntity, SpellID, target.Entity, alwaysUseMP: true) == TurnResult.Succeeded;
        }
    }

    public sealed class InvokeActionAction : BaseAction
    {
        [Dependency] private readonly IVanillaAISystem _vai = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;
        [Dependency] private readonly IActionSystem _actions = default!;

        [DataField]
        [VisualAIVariable]
        public PrototypeId<ActionPrototype> ActionID { get; set; } = Protos.Action.Curse;

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            if (!EntityManager.IsAlive(target.Entity))
                return false;

            if (!_actions.HasAction(state.AIEntity, ActionID))
                return false;

            _vai.SetTarget(state.AIEntity, target.Entity.Value);
            return _actions.Invoke(state.AIEntity, ActionID, target.Entity) == TurnResult.Succeeded;
        }
    }

    public abstract class BaseVerbAction : BaseAction
    {
        [Dependency] private readonly IVerbSystem _verbs = default!;

        protected abstract string VerbType { get; }

        public override bool Apply(VisualAIState state, VisualAIBlock block, IVisualAITargetValue target)
        {
            if (!EntityManager.IsAlive(target.Entity))
                return false;

            var spatialSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            if (spatialSource.MapPosition != target.Coordinates)
                return false;

            var result = TurnResult.Failed;
            if (_verbs.TryGetVerb(state.AIEntity, target.Entity.Value, VerbType, out var verb))
                result = verb.Act();

            return result == TurnResult.Succeeded;
        }
    }

    public sealed class PickUpAction : BaseVerbAction
    {
        protected override string VerbType => PickableSystem.VerbTypePickUp;
    }

    public sealed class DropAction : BaseVerbAction
    {
        protected override string VerbType => PickableSystem.VerbTypeDrop;
    }
}
using ICSharpCode.Decompiler.IL;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Actions;
using OpenNefia.Core.Random;

namespace OpenNefia.VisualAI.Block
{
    // TODO: A lot of these are "comparison/threshold"-type conditions.
    // These have also cropped up in the past with the dialog system,
    // like with comparing the player's current gold/fame from YAML.
    // I believe the two should be combined somehow. That would open
    // up an absurd amount of reuse for anything that needs simple
    // variable retrieval and comparisons through YAML.

    [ImplicitDataDefinitionForInheritors]
    public interface IVisualAICondition
    {
        bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate);
    }

    public abstract class BaseCondition : IVisualAICondition
    {
        [Dependency] protected readonly IEntityManager _entityManager = default!;
        public IEntityManager EntityManager => _entityManager;

        public abstract bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate);
    }

    public sealed class AcceptAllCondition : BaseCondition
    {
        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return true;
        }
    }

    public sealed class AcceptNoneCondition : BaseCondition
    {
        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return false;
        }
    }

    public sealed class IsInFovCondition : BaseCondition
    {
        [Dependency] private readonly IVisibilitySystem _vis = default!;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            var spatialSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var spatialTarget = EntityManager.GetComponent<SpatialComponent>(candidate.Entity.Value);
            var dist = double.Floor((spatialTarget.LocalPosition - spatialSource.LocalPosition).Length);
            var fov = EntityManager.GetComponentOrNull<VisibilityComponent>(state.AIEntity)?.FieldOfViewRadius.Buffed ?? 14;

            return EntityManager.IsAlive(candidate.Entity)
                && _vis.HasLineOfSight(state.AIEntity, candidate.Entity.Value)
                && dist <= fov / 2;
        }
    }

    public sealed class IsPlayerCondition : BaseCondition
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return EntityManager.IsAlive(candidate.Entity)
                && _gameSession.IsPlayer(candidate.Entity.Value);
        }
    }

    public sealed class IsSelfCondition : BaseCondition
    {
        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return EntityManager.IsAlive(candidate.Entity)
                && candidate.Entity == state.AIEntity;
        }
    }

    public sealed class RelationCondition : BaseCondition
    {
        [Dependency] private readonly IFactionSystem _factions = default!;

        [DataField]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        [DataField]
        public Relation Relation { get; set; } = Relation.Neutral;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            // XXX: convert into variable?
            // func(source, target) -> value
            var relation = _factions.GetRelationTowards(state.AIEntity, candidate.Entity.Value);

            return ComparisonUtils.EvaluateComparison(Convert.ToInt32(relation), Convert.ToInt32(Relation), Comparison);
        }
    }

    public sealed class IsAllyCondition : BaseCondition
    {
        [Dependency] private readonly IFactionSystem _factions = default!;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            var relation = _factions.GetRelationTowards(state.AIEntity, candidate.Entity.Value);
            return relation == Relation.Ally;
        }
    }

    public sealed class IsEnemyCondition : BaseCondition
    {
        [Dependency] private readonly IFactionSystem _factions = default!;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            var relation = _factions.GetRelationTowards(state.AIEntity, candidate.Entity.Value);
            return relation == Relation.Enemy;
        }
    }

    public enum EnergyType
    {
        HP,
        MP,
        Stamina
    }

    public sealed class EnergyCondition : BaseCondition
    {
        [Dependency] private readonly IFactionSystem _factions = default!;

        [DataField]
        [VisualAIVariable]
        public EnergyType Type { get; set; }

        [DataField]
        [VisualAIVariable(minValue: 0.0f, maxValue: 1.0f, incrementAmount: 0.1f)]
        public float Threshold { get; set; } = 0.1f;

        [DataField]
        [VisualAIVariable]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        private static float GetEnergyRatio(SkillsComponent skills, EnergyType type)
        {
            switch (type)
            {
                case EnergyType.HP:
                    return skills.HP / skills.MaxHP;
                case EnergyType.MP:
                    return skills.MP / skills.MaxMP;
                case EnergyType.Stamina:
                    return skills.Stamina / skills.Stamina;
                default:
                    throw new InvalidOperationException($"Invalid energy type {type})");
            }
        }

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity) || !EntityManager.TryGetComponent<SkillsComponent>(candidate.Entity.Value, out var skills))
                return false;

            var value = GetEnergyRatio(skills, Type);

            return ComparisonUtils.EvaluateComparison(value, Threshold, Comparison);
        }
    }

    /// <summary>
    /// Selects the player's targeted character. If none is targeted, targets
    /// the player instead.
    /// </summary>
    public sealed class PlayerTargetingCondition : BaseCondition
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVanillaAISystem _vanillaAIs = default!;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            var target = _vanillaAIs.GetTarget(_gameSession.Player) ?? _gameSession.Player;
            return EntityManager.IsAlive(target) && candidate.Entity == target;
        }
    }

    /// <summary>
    /// Targets something, but only in the specified map.
    /// </summary>
    public sealed class SpecificMapCondition : BaseCondition
    {
        [DataField]
        [VisualAIVariable]
        public MapId MapID { get; set; }

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return candidate.Coordinates.MapId == MapID;
        }
    }

    /// <summary>
    /// Targets the specified coordinates in the current map.
    /// </summary>
    public sealed class SpecificLocationCondition : BaseCondition
    {
        [DataField]
        [VisualAIVariable]
        public Vector2i Coordinates { get; set; }

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            return candidate.Coordinates.Position == Coordinates;
        }
    }

    public sealed class DistanceCondition : BaseCondition
    {
        [DataField]
        [VisualAIVariable]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        [DataField]
        [VisualAIVariable(minValue: 0)]
        public int Threshold { get; set; } = 3;

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            var coordsSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity).MapPosition;
            var coordsTarget = candidate.Coordinates;
            var dist = double.Floor((coordsSource.Position - coordsTarget.Position).Length);

            return ComparisonUtils.EvaluateComparison(dist, Threshold, Comparison);
        }
    }

    public sealed class CanDoRangedAttackCondition : BaseCondition
    {
        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            var spatialSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var spatialTarget = EntityManager.GetComponent<SpatialComponent>(candidate.Entity.Value);
            var dist = double.Floor((spatialTarget.LocalPosition - spatialSource.LocalPosition).Length);

            return dist <= VanillaAISystem.AIRangedAttackThreshold;
        }
    }

    public sealed class SpellInRangeCondition : BaseCondition
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [DataField]
        [VisualAIVariable] // TODO known spells only
        public PrototypeId<SpellPrototype> SpellID { get; set; }

        public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
        {
            if (!EntityManager.IsAlive(candidate.Entity))
                return false;

            var spatialSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var spatialTarget = EntityManager.GetComponent<SpatialComponent>(candidate.Entity.Value);
            var dist = double.Floor((spatialTarget.LocalPosition - spatialSource.LocalPosition).Length);

            if (!_protos.TryIndex(SpellID, out var spellProto))
                return false;

            return dist <= spellProto.MaxRange;
        }

        public sealed class ActionInRangeCondition : BaseCondition
        {
            [Dependency] private readonly IPrototypeManager _protos = default!;

            [DataField]
            [VisualAIVariable] // TODO known actions only
            public PrototypeId<ActionPrototype> ActionID { get; set; }

            public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
            {
                if (!EntityManager.IsAlive(candidate.Entity))
                    return false;

                var spatialSource = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
                var spatialTarget = EntityManager.GetComponent<SpatialComponent>(candidate.Entity.Value);
                var dist = double.Floor((spatialTarget.LocalPosition - spatialSource.LocalPosition).Length);

                if (!_protos.TryIndex(ActionID, out var actionProto))
                    return false;

                return dist <= actionProto.MaxRange;
            }
        }

        public sealed class RandomChanceCondition : BaseCondition
        {
            [Dependency] private readonly IRandom _rand = default!;

            [DataField]
            [VisualAIVariable(minValue: 0.0f, maxValue: 1.0f, incrementAmount: 0.1f)]
            public float Probability { get; set; } = 0.5f;

            public override bool IsAccepted(VisualAIState state, IVisualAITargetValue candidate)
            {
                return _rand.Prob(Probability);
            }
        }
    }
}
using OpenNefia.Content.Factions;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.VisualAI.Engine;
using System.Collections.Generic;

namespace OpenNefia.VisualAI.Block
{
    [ImplicitDataDefinitionForInheritors]
    public interface IVisualAITargetOrdering
    {
        int Compare(VisualAIState state, IVisualAITargetValue targetA, IVisualAITargetValue targetB);
    }

    public abstract class BaseTargetOrdering : IVisualAITargetOrdering
    {
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        public abstract int Compare(VisualAIState state, IVisualAITargetValue targetA, IVisualAITargetValue targetB);
    }

    public sealed class DistanceTargetOrdering : BaseTargetOrdering
    {
        [DataField]
        [VisualAIVariable]
        public ComparatorType Comparator { get; set; } = ComparatorType.Smallest;

        public override int Compare(VisualAIState state, IVisualAITargetValue targetA, IVisualAITargetValue targetB)
        {
            var sourceCoords = EntityManager.GetComponent<SpatialComponent>(state.AIEntity);
            var coordsA = targetA.Coordinates;
            var coordsB = targetB.Coordinates;

            var distA = (sourceCoords.LocalPosition - coordsA.Position).Length;
            var distB = (sourceCoords.LocalPosition - coordsB.Position).Length;

            return ComparisonUtils.EvaluateComparator(distA, distB, Comparator);
        }
    }

    public sealed class EnergyTargetOrdering : BaseTargetOrdering
    {
        [DataField]
        [VisualAIVariable]
        public EnergyType Type { get; set; }

        [DataField]
        [VisualAIVariable]
        public ComparatorType Comparator { get; set; } = ComparatorType.Smallest;

        private static float GetEnergyRatio(SkillsComponent skills, EnergyType type)
        {
            switch (type)
            {
                case EnergyType.HP:
                    return skills.HP / skills.MaxHP;
                case EnergyType.MP:
                    return skills.MP / skills.MaxMP;
                case EnergyType.Stamina:
                    return skills.Stamina / skills.MaxStamina;
                default:
                    throw new InvalidOperationException($"Invalid energy type {type})");
            }
        }

        public override int Compare(VisualAIState state, IVisualAITargetValue targetA, IVisualAITargetValue targetB)
        {
            if (!EntityManager.IsAlive(targetA.Entity)
                || !EntityManager.IsAlive(targetB.Entity)
                || !EntityManager.TryGetComponent<SkillsComponent>(targetA.Entity.Value, out var skillsA)
                || !EntityManager.TryGetComponent<SkillsComponent>(targetB.Entity.Value, out var skillsB))
                return 0;

            var ratioA = GetEnergyRatio(skillsA, Type);
            var ratioB = GetEnergyRatio(skillsB, Type);

            return ComparisonUtils.EvaluateComparator(ratioA, ratioB, Comparator);
        }
    }
}
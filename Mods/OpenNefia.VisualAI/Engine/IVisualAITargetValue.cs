using OpenNefia.Content.Visibility;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.VisualAI.Engine
{
    [ImplicitDataDefinitionForInheritors]
    public interface IVisualAITargetValue
    {
        bool IsValid(EntityUid aiEntity);
        MapCoordinates Coordinates { get; }
        EntityUid? Entity { get; }
    }

    public sealed class VisualAIEntityTarget : IVisualAITargetValue
    {
        public VisualAIEntityTarget()
        {
        }

        public VisualAIEntityTarget(EntityUid entity)
        {
            Entity = entity;
        }

        [DataField]
        public EntityUid? Entity { get; set; }

        public MapCoordinates Coordinates => IoCManager.Resolve<IEntityManager>()
            .GetComponent<SpatialComponent>(Entity!.Value).MapPosition;

        public bool IsValid(EntityUid aiEntity)
        {
            return EntitySystem.Get<IVisibilitySystem>().HasLineOfSight(aiEntity, Entity!.Value);
        }
    }

    public sealed class VisualAIPositionTarget : IVisualAITargetValue
    {
        public VisualAIPositionTarget()
        {
        }

        public VisualAIPositionTarget(MapCoordinates coordinates)
        {
            Coordinates = coordinates;
        }

        public EntityUid? Entity => null;

        [DataField]
        public MapCoordinates Coordinates { get; set; }

        public bool IsValid(EntityUid aiEntity)
        {
            return EntitySystem.Get<IVisibilitySystem>().HasLineOfSight(aiEntity, Coordinates);
        }
    }
}
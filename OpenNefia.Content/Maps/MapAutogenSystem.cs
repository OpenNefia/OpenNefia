using OpenNefia.Content.Logic;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;

namespace OpenNefia.Content.Maps
{
    public interface IMapAutogenSystem : IEntitySystem
    {
    }

    public sealed class MapAutogenSystem : EntitySystem, IMapAutogenSystem
    {
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;

        public override void Initialize()
        {
            SubscribeComponent<MapAutogenCharasComponent, MapCreatedFromBlueprintEvent>(AutogenCharas, priority: EventPriorities.VeryHigh);
            SubscribeComponent<MapAutogenEntitiesComponent, MapCreatedFromBlueprintEvent>(AutogenEntities, priority: EventPriorities.VeryHigh);
        }

        private void AutogenCharas(EntityUid uid, MapAutogenCharasComponent component, MapCreatedFromBlueprintEvent args)
        {
            for (var i = 0; i < component.Amount; i++)
            {
                _charaGen.GenerateCharaFromMapFilter(args.Map);
            }
        }

        private void AutogenEntities(EntityUid uid, MapAutogenEntitiesComponent component, MapCreatedFromBlueprintEvent args)
        {
            foreach (var spec in component.Specs)
            {
                for (var i = 0; i < spec.Amount; i++)
                {
                    var chara = _charaGen.GenerateChara(args.Map, spec.ProtoId);
                    if (IsAlive(chara))
                    {
                        foreach (var (name, comp) in spec.Components)
                        {
                            AddComponent(chara.Value, _componentFactory, EntityManager, _serialization, name, comp.Mapping, null);
                        }
                    }
                }
            }
        }

        // TODO dedup from EntityFactory
        private static void AddComponent(EntityUid entity,
            IComponentFactory factory,
            IEntityManager entityManager,
            ISerializationManager serManager,
            string compName,
            MappingDataNode data,
            ISerializationContext? context)
        {
            var compReg = factory.GetRegistration(compName);

            if (!entityManager.TryGetComponent(entity, compReg.Type, out var component))
            {
                var newComponent = (Component)factory.GetComponent(compName);
                newComponent.Owner = entity;
                entityManager.AddComponent(entity, newComponent);
                component = newComponent;
            }

            // TODO use this value to support struct components
            serManager.Read(compReg.Type, data, context, value: component);
        }
    }
}
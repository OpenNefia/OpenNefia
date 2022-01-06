using NLua;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public interface IEntityFactory
    {
        void UpdateEntity(MetaDataComponent metaData, EntityPrototype prototype);
        void LocalizeComponents(EntityUid entityUid);
    }

    internal interface IEntityFactoryInternal : IEntityFactory
    {
        void LoadEntity(EntityPrototype? prototype, EntityUid entity, IComponentFactory factory, IEntityLoadContext? context);
    }

    public class EntityFactory : IEntityFactoryInternal
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly IComponentDependencyManager _componentDependencyManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        public void UpdateEntity(MetaDataComponent metaData, EntityPrototype prototype)
        {
            if (prototype.ID != metaData.EntityPrototype?.ID)
            {
                Logger.Error(
                    $"Reloaded prototype used to update entity did not match entity's existing prototype: Expected '{prototype.ID}', got '{metaData.EntityPrototype?.ID}'");
                return;
            }

            var oldPrototype = metaData.EntityPrototype;

            var oldPrototypeComponents = oldPrototype.Components.Keys
                .Where(n => n != "MetaData")
                .Select(name => (name, _componentFactory.GetRegistration(name).Type))
                .ToList();
            var newPrototypeComponents = prototype.Components.Keys
                .Where(n => n != "MetaData")
                .Select(name => (name, _componentFactory.GetRegistration(name).Type))
                .ToList();

            var entity = metaData.Owner;

            var ignoredComponents = new List<string>();

            // Find components to be removed, and remove them
            foreach (var (name, type) in oldPrototypeComponents.Except(newPrototypeComponents))
            {
                if (prototype.Components.Keys.Contains(name))
                {
                    ignoredComponents.Add(name);
                    continue;
                }

                _entityManager.RemoveComponent(entity, type);
            }

            _entityManager.CullRemovedComponents();

            // Add new components
            foreach (var (name, type) in newPrototypeComponents.Where(t => !ignoredComponents.Contains(t.name))
                .Except(oldPrototypeComponents))
            {
                var data = prototype.Components[name];
                var component = (Component)_componentFactory.GetComponent(name);
                component.Owner = entity;
                _componentDependencyManager.OnComponentAdd(entity, component);
                _entityManager.AddComponent(entity, component);
            }

            // Update entity metadata
            metaData.EntityPrototype = prototype;

            LocalizeComponents(entity);
        }

        public void LoadEntity(EntityPrototype? prototype, EntityUid entity, IComponentFactory factory,
            IEntityLoadContext? context) //yeah officer this method right here
        {
            /*YamlObjectSerializer.Context? defaultContext = null;
            if (context == null)
            {
                defaultContext = new PrototypeSerializationContext(prototype);
            }*/

            if (prototype != null)
            {
                foreach (var (name, data) in prototype.Components)
                {
                    var fullData = data;

                    if (context != null)
                    {
                        if (!context.ShouldLoadComponent(name))
                        {
                            continue;
                        }

                        fullData = context.GetComponentData(name, data);
                    }

                    EnsureCompExistsAndDeserialize(entity, factory, name, fullData, context as ISerializationContext);
                }
            }

            if (context != null)
            {
                foreach (var name in context.GetExtraComponentTypes())
                {
                    if (prototype != null && prototype.Components.ContainsKey(name))
                    {
                        // This component also exists in the prototype.
                        // This means that the previous step already caught both the prototype data AND map data.
                        // Meaning that re-running EnsureCompExistsAndDeserialize would wipe prototype data.
                        continue;
                    }

                    var ser = context.GetComponentData(name, null);

                    EnsureCompExistsAndDeserialize(entity, factory, name, ser, context as ISerializationContext);
                }
            }

            LocalizeComponents(entity);
        }

        public void LocalizeComponents(EntityUid entity)
        {
            if (!_localizationManager.TryGetLocalizationData(entity, out var table))
                return;

            foreach (var comp in _entityManager.GetComponents(entity))
            {
                if (comp is IComponentLocalizable compLocalizable)
                {
                    var obj = table[comp.Name];
                    if (obj is LuaTable compLocaleData)
                    {
                        try
                        {
                            compLocalizable.LocalizeFromLua(compLocaleData);
                        }
                        catch (Exception ex)
                        {
                            EntityPrototype? protoId = null;
                            if (_entityManager.TryGetComponent(entity, out MetaDataComponent metaData))
                            {
                                protoId = metaData.EntityPrototype;
                            }
                            Logger.ErrorS("entity.localize", ex, $"Failed to localize component {comp.Name} ({protoId}): {ex}");
                        }
                    }
                }
            }
        }

        private void EnsureCompExistsAndDeserialize(EntityUid entity, IComponentFactory factory, string compName,
            IComponent data, ISerializationContext? context)
        {
            var compType = factory.GetRegistration(compName).Type;

            if (!_entityManager.TryGetComponent(entity, compType, out var component))
            {
                var newComponent = (Component)factory.GetComponent(compName);
                newComponent.Owner = entity;
                _entityManager.AddComponent(entity, newComponent);
                component = newComponent;
            }

            // TODO use this value to support struct components
            _ = _serializationManager.Copy(data, component, context);
        }
    }
}

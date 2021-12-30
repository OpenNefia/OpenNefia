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
        void UpdateEntity(Entity entity, EntityPrototype prototype);
        void LocalizeComponents(Entity entity);
        void LocalizeComponents(EntityUid entityUid);
    }

    internal interface IEntityFactoryInternal : IEntityFactory
    {
        void LoadEntity(EntityPrototype? prototype, Entity entity, IComponentFactory factory, IEntityLoadContext? context);
    }

    public class EntityFactory : IEntityFactoryInternal
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly IComponentDependencyManager _componentDependencyManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        public void UpdateEntity(Entity entity, EntityPrototype prototype)
        {
            if (prototype.ID != entity.Prototype?.ID)
            {
                Logger.Error(
                    $"Reloaded prototype used to update entity did not match entity's existing prototype: Expected '{prototype.ID}', got '{entity.Prototype?.ID}'");
                return;
            }

            var oldPrototype = entity.Prototype;

            var oldPrototypeComponents = oldPrototype.Components.Keys
                .Where(n => n != "MetaData")
                .Select(name => (name, _componentFactory.GetRegistration(name).Type))
                .ToList();
            var newPrototypeComponents = prototype.Components.Keys
                .Where(n => n != "MetaData")
                .Select(name => (name, _componentFactory.GetRegistration(name).Type))
                .ToList();

            var ignoredComponents = new List<string>();

            // Find components to be removed, and remove them
            foreach (var (name, type) in oldPrototypeComponents.Except(newPrototypeComponents))
            {
                if (prototype.Components.Keys.Contains(name))
                {
                    ignoredComponents.Add(name);
                    continue;
                }

                _entityManager.RemoveComponent(entity.Uid, type);
            }

            _entityManager.CullRemovedComponents();

            // Add new components
            foreach (var (name, type) in newPrototypeComponents.Where(t => !ignoredComponents.Contains(t.name))
                .Except(oldPrototypeComponents))
            {
                var data = prototype.Components[name];
                var component = (Component)_componentFactory.GetComponent(name);
                component.Owner = entity;
                _componentDependencyManager.OnComponentAdd(entity.Uid, component);
                entity.AddComponent(component);
            }

            // Update entity metadata
            entity.MetaData.EntityPrototype = prototype;

            LocalizeComponents(entity);
        }

        public void LoadEntity(EntityPrototype? prototype, Entity entity, IComponentFactory factory,
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

        public void LocalizeComponents(EntityUid entityUid)
        {
            if (!_entityManager.TryGetEntity(entityUid, out var entity))
                return;

            LocalizeComponents(entity);
        }

        public void LocalizeComponents(Entity entity)
        {
            if (!_localizationManager.TryGetLocalizationData(entity.Uid, out var table))
                return;

            foreach (var comp in entity.GetAllComponents())
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
                            var proto = entity.MetaData.EntityPrototype;
                            Logger.ErrorS("entity.localize", ex, $"Failed to localize component {comp.Name} ({proto}): {ex}");
                        }
                    }
                }
            }
        }

        private void EnsureCompExistsAndDeserialize(Entity entity, IComponentFactory factory, string compName,
            IComponent data, ISerializationContext? context)
        {
            var compType = factory.GetRegistration(compName).Type;

            if (!entity.TryGetComponent(compType, out var component))
            {
                var newComponent = (Component)factory.GetComponent(compName);
                newComponent.Owner = entity;
                entity.AddComponent(newComponent);
                component = newComponent;
            }

            // TODO use this value to support struct components
            _ = _serializationManager.Copy(data, component, context);
        }
    }
}

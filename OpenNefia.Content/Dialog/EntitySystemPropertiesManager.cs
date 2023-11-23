using OpenNefia.Core.GameObjects;
using System.Reflection;

namespace OpenNefia.Content.Dialog
{
    /// <summary>
    /// Reference to a property in an <see cref="IEntitySystem"/> that can be modified indirectly.
    /// </summary>
    /// <param name="EntitySystem"></param>
    /// <param name="Property"></param>
    public sealed record class EntitySystemProperty(IEntitySystem EntitySystem, PropertyInfo Property)
    {
        public T GetValue<T>()
        {
            if (Property.PropertyType != typeof(T))
                throw new InvalidDataException($"Property {this} is not of type {typeof(T)}.");

            return (T)Property.GetValue(EntitySystem)!;
        }

        public object GetValue()
        {
            return Property.GetValue(EntitySystem)!;
        }

        public void SetValue<T>(T value)
        {
            if (Property.PropertyType != typeof(T))
                throw new InvalidDataException($"Property {this} is not of type {typeof(T)}.");

            Property.SetValue(EntitySystem, value);
        }

        public override string ToString()
        {
            return $"{EntitySystem.GetType().FullName}:{Property.Name}";
        }
    }

    /// <summary>
    /// Facilitates retreival of state via string references to entity system property names in YAML.
    /// </summary>
    /// <remarks>
    /// Could be expanded later to support properties of entities also.
    /// </remarks>
    public interface IEntitySystemPropertiesManager
    {
        /// <summary>
        /// Retrieves the property of an entity system referenced from YAML.
        /// </summary>
        /// <param name="propRef">Namespaced type and property  name</param>
        /// <returns>The retreived property</returns>
        EntitySystemProperty GetProperty(EntitySystemPropertyRef propRef);
    }

    public sealed class EntitySystemPropertiesManager : IEntitySystemPropertiesManager
    {
        public EntitySystemProperty GetProperty(EntitySystemPropertyRef propRef)
        {
            if (!propRef.IsValid())
                throw new InvalidDataException($"Invalid entity system property reference: {propRef}");

            if (!typeof(IEntitySystem).IsAssignableFrom(propRef.SystemType))
                throw new InvalidDataException($"{propRef.SystemType} does not implement {nameof(IEntitySystem)}.");

            var system = EntitySystem.Get(propRef.SystemType);
            var property = propRef.SystemType.GetProperty(propRef.PropertyName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
                throw new InvalidDataException($"{propRef.SystemType} does not have a public instance field named {propRef.PropertyName}.");

            return new EntitySystemProperty(system, property);
        }
    }
}
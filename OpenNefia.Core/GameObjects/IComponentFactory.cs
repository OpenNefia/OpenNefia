using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Handles the registration and spawning of components.
    /// </summary>
    /// <remarks>
    /// <p>
    /// When referring to component names, this is the name the component has been registered as,
    /// and what's used in prototypes. However, most commonly the type is referred through by an interface.
    /// </p>
    /// <p>
    /// Before a component can be spawned, it must be registered so things such as name, networking ID, type, etc...
    /// are known to the factory.
    /// Components are registered into a registry.
    /// The relevant methods for writing to this registry are <see cref="RegisterReference" />.
    /// The data is exposed for reading through <see cref="GetRegistration" /> and its overloads.
    /// This data is returned in the form of a <see cref="IComponentRegistration" />, which represents one component's registration.
    /// </p>
    /// </remarks>
    /// <seealso cref="IComponentRegistration" />
    /// <seealso cref="IComponent" />
    public interface IComponentFactory
    {
        event Action<IComponentRegistration> ComponentAdded;
        event Action<(IComponentRegistration, Type)> ComponentReferenceAdded;
        event Action<string> ComponentIgnoreAdded;

        /// <summary>
        ///     All IComponent types that are currently registered to this factory.
        /// </summary>
        IEnumerable<Type> AllRegisteredTypes { get; }

        /// <summary>
        /// Registers the default components.
        /// </summary>
        public void DoDefaultRegistrations();

        public bool IsRegistered(string name);

        /// <summary>
        /// Registers a component class with the factory.
        /// </summary>
        /// <param name="overwrite">If the component already exists, will this replace it?</param>
        void RegisterClass<T>(bool overwrite = false) where T : IComponent, new();

        /// <summary>
        /// Registers a component name as being ignored.
        /// </summary>
        /// <param name="name">The name to be ignored.</param>
        /// <param name="overwrite">Whether to overrde existing settings instead of throwing an exception in the case of duplicates.</param>
        void RegisterIgnore(string name, bool overwrite = false);

        /// <summary>
        /// Gets a new component instantiated of the specified type.
        /// </summary>
        /// <param name="componentType">type of component to make</param>
        /// <returns>A Component</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if no component of type <see cref="componentType"/> is registered.
        /// </exception>
        IComponent GetComponent(Type componentType);

        /// <summary>
        /// Gets a new component instantiated of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to make.</typeparam>
        /// <returns>A Component</returns>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if no component of type <see cref="T"/> is registered.
        /// </exception>
        T GetComponent<T>() where T : IComponent, new();

        /// <summary>
        /// Gets a new component instantiated of the specified <see cref="IComponent.Name"/>.
        /// </summary>
        /// <param name="componentName">name of component to make</param>
        /// <param name="ignoreCase">Whether or not to ignore casing on <see cref="componentName"/></param>
        /// <returns>A Component</returns>
        /// <exception cref="UnknownComponentException">
        ///     Thrown if no component exists with the given name <see cref="componentName"/>.
        /// </exception>
        IComponent GetComponent(string componentName, bool ignoreCase = false);

        /// <summary>
        ///     Gets the registration belonging to a component, throwing an exception if it does not exist.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="ignoreCase">Whether or not to ignore casing on <see cref="componentName"/></param>
        /// <exception cref="UnknownComponentException">
        ///     Thrown if no component exists with the given name <see cref="componentName"/>.
        /// </exception>
        IComponentRegistration GetRegistration(string componentName, bool ignoreCase = false);

        /// <summary>
        ///     Gets the registration belonging to a component, throwing an exception if it does not exist.
        /// </summary>
        /// <param name="reference">The type of the component to lookup.</param>
        /// <exception cref="UnknownComponentException">
        ///     Thrown if no component exists of type <see cref="reference"/>.
        /// </exception>
        IComponentRegistration GetRegistration(Type reference);

        /// <summary>
        ///     Gets the registration belonging to a component, throwing an exception if it does not exist.
        /// </summary>
        /// <typeparam name="T">A type referencing the component.</typeparam>
        /// <exception cref="UnknownComponentException">
        ///     Thrown if no component of type <see cref="T"/> exists.
        /// </exception>
        IComponentRegistration GetRegistration<T>() where T : IComponent, new();

        /// <summary>
        ///     Gets the registration of a component, throwing an exception if
        ///     it does not exist.
        /// </summary>
        /// <param name="component">An instance of the component.</param>
        /// <returns></returns>
        /// <exception cref="UnknownComponentException">
        ///     Thrown if no registration exists for component <see cref="component"/>.
        /// </exception>
        IComponentRegistration GetRegistration(IComponent component);

        /// <summary>
        ///     Tries to get the registration belonging to a component.
        /// </summary>
        /// <param name="componentName">The name of the component.</param>
        /// <param name="registration">The registration if found, null otherwise.</param>
        /// <param name="ignoreCase">Whether or not to ignore casing on <see cref="componentName"/></param>
        /// <returns>true it found, false otherwise.</returns>
        bool TryGetRegistration(string componentName, [NotNullWhen(true)] out IComponentRegistration? registration, bool ignoreCase = false);

        /// <summary>
        ///     Tries to get the registration belonging to a component.
        /// </summary>
        /// <param name="reference">A reference corresponding to the component to look up.</param>
        /// <param name="registration">The registration if found, null otherwise.</param>
        /// <returns>true it found, false otherwise.</returns>
        bool TryGetRegistration(Type reference, [NotNullWhen(true)] out IComponentRegistration? registration);

        /// <summary>
        ///     Tries to get the registration belonging to a component.
        /// </summary>
        /// <typeparam name="T">A type referencing the component.</typeparam>
        /// <param name="registration">The registration if found, null otherwise.</param>
        /// <returns>true it found, false otherwise.</returns>
        bool TryGetRegistration<T>([NotNullWhen(true)] out IComponentRegistration? registration) where T : IComponent, new();

        /// <summary>
        ///     Tries to get the registration of a component.
        /// </summary>
        /// <param name="component">An instance of the component.</param>
        /// <param name="registration">The registration if found, null otherwise.</param>
        /// <returns>true it found, false otherwise.</returns>
        bool TryGetRegistration(IComponent component, [NotNullWhen(true)] out IComponentRegistration? registration);

        /// <summary>
        ///     Automatically create registrations for all components with a <see cref="RegisterComponentAttribute" />
        /// </summary>
        void DoAutoRegistrations();

        IEnumerable<Type> GetAllRefTypes();

        void FinishRegistration();
    }

    /// <summary>
    /// Represents a component registered into a <see cref="IComponentFactory" />.
    /// </summary>
    /// <seealso cref="IComponentFactory" />
    /// <seealso cref="IComponent" />
    public interface IComponentRegistration
    {
        /// <summary>
        /// The name of the component.
        /// This is used as the <c>type</c> field in the component declarations if entity prototypes.
        /// </summary>
        /// <seealso cref="IComponent.Name" />
        string Name { get; }

        /// <summary>
        /// The type that will be instantiated if this component is created.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// A list of type references that can be used to get a reference to an instance of this component,
        /// for methods like <see cref="Entity.GetComponent{T}" />.
        /// These are not unique and can overlap with other components.
        /// </summary>
        IReadOnlyList<Type> References { get; }
    }
}

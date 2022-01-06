using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace OpenNefia.Core.GameObjects
{
    public partial interface IEntityManager
    {
        /// <summary>
        ///     A component was added to the manager.
        /// </summary>
        event EventHandler<ComponentEventArgs>? ComponentAdded;

        /// <summary>
        ///     A component was removed from the manager.
        /// </summary>
        event EventHandler<ComponentEventArgs>? ComponentRemoved;

        /// <summary>
        ///     A component was deleted. This is usually deferred until some time after it was removed.
        ///     Usually you will want to subscribe to <see cref="ComponentRemoved"/>.
        /// </summary>
        event EventHandler<ComponentEventArgs>? ComponentDeleted;

        /// <summary>
        ///     Calls Initialize() on all registered components of the entity.
        /// </summary>
        void InitializeComponents(EntityUid uid);

        /// <summary>
        ///     Calls Startup() on all registered components of the entity.
        /// </summary>
        void StartComponents(EntityUid uid);

        /// <summary>
        ///     Adds a Component type to an entity. If the entity is already Initialized, the component will
        ///     automatically be Initialized and Started.
        /// </summary>
        /// <typeparam name="T">Concrete component type to add.</typeparam>
        /// <returns>The newly added component.</returns>
        T AddComponent<T>(EntityUid uid) where T : Component, new();

        /// <summary>
        ///     Adds a Component type to an entity. If the entity is already Initialized, the component will
        ///     automatically be Initialized and Started.
        /// </summary>
        /// <param name="type">Concrete component type to add.</typeparam>
        /// <returns>The newly added component.</returns>
        IComponent AddComponent(EntityUid uid, Type type);

        /// <summary>
        ///     Adds a Component to an entity. If the entity is already Initialized, the component will
        ///     automatically be Initialized and Started.
        /// </summary>
        /// <param name="uid">Entity being modified.</param>
        /// <param name="component">Component to add.</param>
        /// <param name="overwrite">Should it overwrite existing components?</param>
        void AddComponent<T>(EntityUid uid, T component, bool overwrite = false) where T : Component;

        /// <summary>
        ///     Removes the component with the specified reference type,
        ///     Without needing to have the component itself.
        /// </summary>
        /// <typeparam name="T">The component reference type to remove.</typeparam>
        /// <param name="uid">Entity UID to modify.</param>
        void RemoveComponent<T>(EntityUid uid);

        /// <summary>
        ///     Removes the component with a specified type.
        /// </summary>
        /// <param name="uid">Entity UID to modify.</param>
        /// <param name="type">A trait or component type to check for.</param>
        void RemoveComponent(EntityUid uid, Type type);

        /// <summary>
        ///     Removes the specified component.
        /// </summary>
        /// <param name="uid">Entity UID to modify.</param>
        /// <param name="component">Component to remove.</param>
        void RemoveComponent(EntityUid uid, IComponent component);

        /// <summary>
        ///     Removes all components from an entity, except the required components.
        /// </summary>
        /// <param name="uid">Entity UID to modify.</param>
        void RemoveComponents(EntityUid uid);

        /// <summary>
        ///     Removes ALL components from an entity. This includes the required components,
        ///     <see cref="SpatialComponent"/> and <see cref="MetaDataComponent"/>. This should ONLY be
        ///     used when deleting an entity.
        /// </summary>
        /// <param name="uid">Entity UID to modify.</param>
        void DisposeComponents(EntityUid uid);

        /// <summary>
        ///     Checks if the entity has a component type.
        /// </summary>
        /// <typeparam name="T">Component reference type to check for.</typeparam>
        /// <param name="uid">Entity UID to check.</param>
        /// <returns>True if the entity has the component type, otherwise false.</returns>
        bool HasComponent<T>(EntityUid uid);

        /// <summary>
        ///     Checks if the entity has a component type.
        /// </summary>
        /// <param name="uid">Entity UID to check.</param>
        /// <param name="type">A trait or component type to check for.</param>
        /// <returns>True if the entity has the component type, otherwise false.</returns>
        bool HasComponent(EntityUid uid, Type type);

        /// <summary>
        ///     This method will always return a component for a certain entity, adding it if it's not there already.
        /// </summary>
        /// <param name="uid">Entity to modify.</param>
        /// <typeparam name="T">Component to add.</typeparam>
        /// <returns>The component in question</returns>
        T EnsureComponent<T>(EntityUid uid) where T : Component, new();

        /// <summary>
        ///     This method will always return a component for a certain entity, adding it if it's not there already.
        /// </summary>
        /// <param name="uid">Entity to modify.</param>
        /// <param name="type">Component to add.</typeparam>
        /// <returns>The component in question</returns>
        IComponent EnsureComponent(EntityUid uid, Type type);

        /// <summary>
        ///     Returns the component of a specific type.
        /// </summary>
        /// <remarks>
        /// *Strongly prefer* <see cref="TryGetComponent{T}(EntityUid, out T)"/>.
        /// </remarks>
        /// <typeparam name="T">A trait or type of a component to retrieve.</typeparam>
        /// <param name="uid">Entity UID to look on.</param>
        /// <returns>The component of Type from the Entity.</returns>
        T GetComponent<T>(EntityUid uid);

        /// <summary>
        ///     Returns the component of a specific type.
        /// </summary>
        /// <remarks>
        /// *Strongly prefer* <see cref="TryGetComponent(EntityUid, Type, out IComponent?)"/>
        /// </remarks>
        /// <param name="uid">Entity UID to look on.</param>
        /// <param name="type">A trait or component type to check for.</param>
        /// <returns>The component of Type from the Entity.</returns>
        IComponent GetComponent(EntityUid uid, Type type);

        /// <summary>
        ///     Returns the component of a specific type.
        /// </summary>
        /// <typeparam name="T">A trait or type of a component to retrieve.</typeparam>
        /// <param name="uid">Entity UID to check.</param>
        /// <param name="component">Component of the specified type (if exists).</param>
        /// <returns>If the component existed in the entity.</returns>
        bool TryGetComponent<T>(EntityUid uid, [NotNullWhen(true)] out T component);

        /// <summary>
        ///     Returns the component of a specific type.
        /// </summary>
        /// <param name="uid">Entity UID to check.</param>
        /// <param name="type">A trait or component type to check for.</param>
        /// <param name="component">Component of the specified type (if exists).</param>
        /// <returns>If the component existed in the entity.</returns>
        bool TryGetComponent(EntityUid uid, Type type, [NotNullWhen(true)] out IComponent? component);

        /// <summary>
        ///     Returns ALL component type instances on an entity. A single component instance
        ///     can have multiple component types.
        /// </summary>
        /// <param name="uid">Entity UID to look on.</param>
        /// <returns>All component types on the Entity.</returns>
        IEnumerable<IComponent> GetComponents(EntityUid uid);

        /// <summary>
        ///     Returns ALL component type instances that are assignable to the specified type.
        ///     A single component instance can have multiple component type instances.
        /// </summary>
        /// <typeparam name="T">A trait or type of a component to retrieve.</typeparam>
        /// <param name="uid">Entity UID to look on.</param>
        /// <returns>All components that are assignable to the specified type.</returns>
        IEnumerable<T> GetComponents<T>(EntityUid uid);

        /// <summary>
        ///     Returns ALL component instances of a specified type.
        /// </summary>
        /// <typeparam name="T">A trait or type of a component to retrieve.</typeparam>
        /// <returns>All components that have the specified type.</returns>
        IEnumerable<T> EntityQuery<T>();

        /// <summary>
        /// Returns the relevant components from all entities that contain the two required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <returns>The pairs of components from each entity that has the two required components.</returns>
        IEnumerable<(TComp1, TComp2)> EntityQuery<TComp1, TComp2>()
            where TComp1 : IComponent
            where TComp2 : IComponent;

        /// <summary>
        /// Returns the relevant components from all entities that contain the three required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <typeparam name="TComp3">Third required component.</typeparam>
        /// <returns>The pairs of components from each entity that has the three required components.</returns>
        IEnumerable<(TComp1, TComp2, TComp3)> EntityQuery<TComp1, TComp2, TComp3>()
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent;

        /// <summary>
        /// Returns the relevant components from all entities that contain the four required components.
        /// </summary>
        /// <typeparam name="TComp1">First required component.</typeparam>
        /// <typeparam name="TComp2">Second required component.</typeparam>
        /// <typeparam name="TComp3">Third required component.</typeparam>
        /// <typeparam name="TComp4">Fourth required component.</typeparam>
        /// <returns>The pairs of components from each entity that has the four required components.</returns>
        IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQuery<TComp1, TComp2, TComp3, TComp4>()
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent;

        /// <summary>
        ///      Returns ALL component instances of a specified type.
        /// </summary>
        /// <param name="type">A trait or component type to check for.</param>
        /// <param name="includePaused"></param>
        /// <returns>All components that are the specified type.</returns>
        IEnumerable<IComponent> GetAllComponents(Type type);

        /// <summary>
        ///      Returns ALL component instances of a specified type.
        /// </summary>
        /// <param name="type">A trait or component type to check for.</param>
        /// <param name="includePaused"></param>
        /// <returns>All components that are the specified type.</returns>
        IEnumerable<T> GetAllComponents<T>() where T : IComponent;

        /// <summary>
        ///     Culls all components from the collection that are marked as deleted. This needs to be called often.
        /// </summary>
        void CullRemovedComponents();
    }
}

﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.GameObjects
{
    public interface IEntity
    {
        /// <summary>
        /// The Entity Manager that controls this entity.
        /// </summary>
        IEntityManager EntityManager { get; }

        /// <summary>
        /// The unique ID of this entity.
        /// </summary>
        EntityUid Uid { get; }

        /// <summary>
        ///     The current lifetime stage of this entity. You can use this to check
        ///     if the entity is initialized or being deleted.
        /// </summary>
        EntityLifeStage LifeStage { get; internal set; }

        /// <summary>
        ///     The prototype that was used to create this entity.
        /// </summary>
        EntityPrototype? Prototype { get; }

        /// <summary>
        ///     The Transform Component of this entity.
        /// </summary>
        SpatialComponent Spatial { get; }

        /// <summary>
        ///     The MetaData Component of this entity.
        /// </summary>
        MetaDataComponent MetaData { get; }

        /// <summary>
        ///     Whether this entity has fully initialized.
        /// </summary>
        bool Initialized { get; }

        bool Initializing { get; }

        /// <summary>
        ///     True if the entity has been deleted.
        /// </summary>
        bool Deleted { get; }

        /// <summary>
        ///     Determines if this entity is still valid.
        /// </summary>
        /// <returns>True if this entity is still valid.</returns>
        bool IsValid();

        /// <summary>
        ///     Public method to add a component to an entity.
        ///     Calls the component's onAdd method, which also adds it to the component manager.
        /// </summary>
        /// <typeparam name="T">The component type to add.</typeparam>
        /// <returns>The newly added component.</returns>
        T AddComponent<T>()
            where T : Component, new();

        /// <summary>
        ///     Removes the component with the specified reference type,
        ///     Without needing to have the component itself.
        /// </summary>
        /// <typeparam name="T">The component reference type to remove.</typeparam>
        void RemoveComponent<T>();

        /// <summary>
        ///     Checks to see if the entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component reference type to check.</typeparam>
        /// <returns>True if the entity has a component of type <typeparamref name="T" />, false otherwise.</returns>
        bool HasComponent<T>();

        /// <summary>
        ///     Checks to see ift he entity has a component of the specified type.
        /// </summary>
        /// <param name="type">The component reference type to check.</param>
        /// <returns></returns>
        bool HasComponent(Type type);

        /// <summary>
        ///     Retrieves the component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component reference type to fetch.</typeparam>
        /// <returns>The retrieved component.</returns>
        /// <exception cref="Shared.GameObjects.UnknownComponentException">
        ///     Thrown if there is no component with the specified type.
        /// </exception>
        T GetComponent<T>();

        /// <summary>
        ///     Retrieves the component of the specified type.
        /// </summary>
        /// <param name="type">The component reference type to fetch.</param>
        /// <returns>The retrieved component.</returns>
        /// <exception cref="Shared.GameObjects.UnknownComponentException">
        ///     Thrown if there is no component with the specified type.
        /// </exception>
        IComponent GetComponent(Type type);

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     writing it to the <paramref name="component" /> out parameter if it was found.
        /// </summary>
        /// <typeparam name="T">The component reference type to attempt to fetch.</typeparam>
        /// <param name="component">The component, if it was found. Null otherwise.</param>
        /// <returns>True if a component with specified type was found.</returns>
        bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class;

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     returning it if it was found.
        /// </summary>
        /// <typeparam name="T">The component reference type to attempt to fetch.</typeparam>
        /// <returns>The component, if it was found. Null otherwise.</returns>
        T? GetComponentOrNull<T>() where T : class;

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     writing it to the <paramref name="component" /> out parameter if it was found.
        /// </summary>
        /// <param name="type">The component reference type to attempt to fetch.</param>
        /// <param name="component">The component, if it was found. Null otherwise.</param>
        /// <returns>True if a component with specified type was found.</returns>
        bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component);

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     returning it if it was found.
        /// </summary>
        /// <param name="type">The component reference type to attempt to fetch.</param>
        /// <returns>The component, if it was found. Null otherwise.</returns>
        IComponent? GetComponentOrNull(Type type);

        /// <summary>
        ///     Deletes this entity.
        /// </summary>
        void Delete();

        /// <summary>
        ///     Returns all components on the entity.
        /// </summary>
        /// <returns>An enumerable of components on the entity.</returns>
        IEnumerable<IComponent> GetAllComponents();

        /// <summary>
        ///     Returns all components that are assignable to <typeparamref name="T"/>.
        ///     This does not go by component references.
        /// </summary>
        /// <typeparam name="T">The type that components must implement.</typeparam>
        /// <returns>An enumerable over the found components.</returns>
        IEnumerable<T> GetAllComponents<T>();
    }
}

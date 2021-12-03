using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    /// <inheritdoc />
    public sealed class Entity
    {
        #region Members

        /// <summary>
        /// The Entity Manager that controls this entity.
        /// </summary>
        public IEntityManager EntityManager { get; }

        /// <summary>
        /// The unique ID of this entity.
        /// </summary>
        public EntityUid Uid { get; }

        /// <summary>
        ///     The current lifetime stage of this entity. You can use this to check
        ///     if the entity is initialized or being deleted.
        /// </summary>
        public EntityLifeStage LifeStage { get => MetaData.EntityLifeStage; internal set => MetaData.EntityLifeStage = value; }

        /// <summary>
        ///     The prototype that was used to create this entity.
        /// </summary>
        public EntityPrototype? Prototype
        {
            get => MetaData.EntityPrototype;
            internal set => MetaData.EntityPrototype = value;
        }

        private SpatialComponent? _spatial;

        /// <summary>
        ///     The Spatial Component of this entity.
        /// </summary>
        public SpatialComponent Spatial
        {
            get => _spatial ??= GetComponent<SpatialComponent>();
            internal set => _spatial = value;
        }

        private MetaDataComponent? _metaData;

        /// <summary>
        ///     The MetaData Component of this entity.
        /// </summary>
        public MetaDataComponent MetaData
        {
            get => _metaData ??= GetComponent<MetaDataComponent>();
            internal set => _metaData = value;
        }

        /// <summary>
        ///     Whether this entity has fully initialized.
        /// </summary>
        public bool Initialized => LifeStage >= EntityLifeStage.Initialized;

        /// <inheritdoc />
        public bool Initializing => LifeStage == EntityLifeStage.Initializing;

        /// <summary>
        ///     True if the entity has been deleted.
        /// </summary>
        public bool Deleted => LifeStage >= EntityLifeStage.Deleted;

        #endregion Members

        #region Initialization

        public Entity(IEntityManager entityManager, EntityUid uid)
        {
            EntityManager = entityManager;
            Uid = uid;
        }

        /// <summary>
        ///     Determines if this entity is still valid.
        /// </summary>
        /// <returns>True if this entity is still valid.</returns>
        public bool IsValid()
        {
            return EntityManager.EntityExists(Uid);
        }

        #endregion Initialization

        #region Components

        /// <summary>
        ///     Public method to add a component to an entity.
        ///     Calls the component's onAdd method, which also adds it to the component manager.
        /// </summary>
        /// <param name="component">The component to add.</param>
        public void AddComponent(Component component)
        {
            EntityManager.AddComponent(this, component);
        }

        /// <summary>
        ///     Public method to add a component to an entity.
        ///     Calls the component's onAdd method, which also adds it to the component manager.
        /// </summary>
        /// <typeparam name="T">The component type to add.</typeparam>
        /// <returns>The newly added component.</returns>
        public T AddComponent<T>()
            where T : Component, new()
        {
            return EntityManager.AddComponent<T>(this);
        }

        /// <summary>
        ///     Removes the component with the specified reference type,
        ///     Without needing to have the component itself.
        /// </summary>
        /// <typeparam name="T">The component reference type to remove.</typeparam>
        public void RemoveComponent<T>()
        {
            EntityManager.RemoveComponent<T>(Uid);
        }

        /// <summary>
        ///     Checks to see if the entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component reference type to check.</typeparam>
        /// <returns>True if the entity has a component of type <typeparamref name="T" />, false otherwise.</returns>
        public bool HasComponent<T>()
        {
            return EntityManager.HasComponent<T>(Uid);
        }

        /// <summary>
        ///     Checks to see ift he entity has a component of the specified type.
        /// </summary>
        /// <param name="type">The component reference type to check.</param>
        /// <returns></returns>
        public bool HasComponent(Type type)
        {
            return EntityManager.HasComponent(Uid, type);
        }

        /// <summary>
        ///     Retrieves the component of the specified type.
        /// </summary>
        /// <typeparam name="T">The component reference type to fetch.</typeparam>
        /// <returns>The retrieved component.</returns>
        /// <exception cref="Shared.GameObjects.UnknownComponentException">
        ///     Thrown if there is no component with the specified type.
        /// </exception>
        public T GetComponent<T>()
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return (T)EntityManager.GetComponent(Uid, typeof(T));
        }

        /// <summary>
        ///     Retrieves the component of the specified type.
        /// </summary>
        /// <param name="type">The component reference type to fetch.</param>
        /// <returns>The retrieved component.</returns>
        /// <exception cref="Shared.GameObjects.UnknownComponentException">
        ///     Thrown if there is no component with the specified type.
        /// </exception>
        public IComponent GetComponent(Type type)
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.GetComponent(Uid, type);
        }

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     writing it to the <paramref name="component" /> out parameter if it was found.
        /// </summary>
        /// <typeparam name="T">The component reference type to attempt to fetch.</typeparam>
        /// <param name="component">The component, if it was found. Null otherwise.</param>
        /// <returns>True if a component with specified type was found.</returns>
        public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.TryGetComponent(Uid, out component);
        }

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     returning it if it was found.
        /// </summary>
        /// <typeparam name="T">The component reference type to attempt to fetch.</typeparam>
        /// <returns>The component, if it was found. Null otherwise.</returns>
        public T? GetComponentOrNull<T>() where T : class
        {
            return TryGetComponent(out T? component) ? component : default;
        }

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     writing it to the <paramref name="component" /> out parameter if it was found.
        /// </summary>
        /// <param name="type">The component reference type to attempt to fetch.</param>
        /// <param name="component">The component, if it was found. Null otherwise.</param>
        /// <returns>True if a component with specified type was found.</returns>
        public bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component)
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.TryGetComponent(Uid, type, out component);
        }

        /// <summary>
        ///     Attempt to retrieve the component with specified type,
        ///     returning it if it was found.
        /// </summary>
        /// <param name="type">The component reference type to attempt to fetch.</param>
        /// <returns>The component, if it was found. Null otherwise.</returns>
        public IComponent? GetComponentOrNull(Type type)
        {
            return TryGetComponent(type, out var component) ? component : null;
        }

        /// <summary>
        ///     Deletes this entity.
        /// </summary>
        public void Delete()
        {
            EntityManager.DeleteEntity(this);
        }

        /// <summary>
        ///     Returns all components on the entity.
        /// </summary>
        /// <returns>An enumerable of components on the entity.</returns>
        public IEnumerable<IComponent> GetAllComponents()
        {
            return EntityManager.GetComponents(Uid);
        }

        /// <summary>
        ///     Returns all components that are assignable to <typeparamref name="T"/>.
        ///     This does not go by component references.
        /// </summary>
        /// <typeparam name="T">The type that components must implement.</typeparam>
        /// <returns>An enumerable over the found components.</returns>
        public IEnumerable<T> GetAllComponents<T>()
        {
            return EntityManager.GetComponents<T>(Uid);
        }

        #endregion Components

        /// <inheritdoc />
        public override string ToString()
        {
            if (Deleted)
            {
                return $"({Uid}, {Prototype?.ID})D";
            }
            return $"({Uid}, {Prototype?.ID})";
        }
    }
}

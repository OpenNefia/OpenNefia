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
    public sealed class Entity : IEntity
    {
        #region Members

        /// <inheritdoc />
        public IEntityManager EntityManager { get; }

        /// <inheritdoc />
        public EntityUid Uid { get; }

        /// <inheritdoc />
        public Vector2i Pos { get; set; }

        /// <inheritdoc />
        public IMap? Map { get; private set; }
        
        /// <inheritdoc />
        public MapCoordinates Coords { get => new MapCoordinates(Map, Pos); }

        /// <inheritdoc />
        EntityLifeStage IEntity.LifeStage { get => LifeStage; set => LifeStage = value; }

        public EntityLifeStage LifeStage { get => MetaData.EntityLifeStage; internal set => MetaData.EntityLifeStage = value; }

        /// <inheritdoc />
        public EntityPrototype? Prototype
        {
            get => MetaData.EntityPrototype;
            internal set => MetaData.EntityPrototype = value;
        }

        /// <inheritdoc />
        public bool Initialized => LifeStage >= EntityLifeStage.Initialized;

        /// <inheritdoc />
        public bool Initializing => LifeStage == EntityLifeStage.Initializing;

        /// <inheritdoc />
        public bool Deleted => LifeStage >= EntityLifeStage.Deleted;

        private MetaDataComponent? _metaData;

        /// <inheritdoc />
        public MetaDataComponent MetaData
        {
            get => _metaData ??= GetComponent<MetaDataComponent>();
            internal set => _metaData = value;
        }

        #endregion Members

        #region Initialization

        public Entity(IEntityManager entityManager, EntityUid uid)
        {
            EntityManager = entityManager;
            Uid = uid;
        }

        /// <inheritdoc />
        public bool IsValid()
        {
            return EntityManager.EntityExists(Uid);
        }

        #endregion Initialization

        internal void ChangeMap(IMap newMap)
        {
            if (newMap == Map)
                return;

            var oldMap = Map;

            Map = newMap;
        }

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

        /// <inheritdoc />
        public T AddComponent<T>()
            where T : Component, new()
        {
            return EntityManager.AddComponent<T>(this);
        }

        /// <inheritdoc />
        public void RemoveComponent<T>()
        {
            EntityManager.RemoveComponent<T>(Uid);
        }

        /// <inheritdoc />
        public bool HasComponent<T>()
        {
            return EntityManager.HasComponent<T>(Uid);
        }

        /// <inheritdoc />
        public bool HasComponent(Type type)
        {
            return EntityManager.HasComponent(Uid, type);
        }

        /// <inheritdoc />
        public T GetComponent<T>()
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return (T)EntityManager.GetComponent(Uid, typeof(T));
        }

        /// <inheritdoc />
        public IComponent GetComponent(Type type)
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.GetComponent(Uid, type);
        }

        /// <inheritdoc />
        public bool TryGetComponent<T>([NotNullWhen(true)] out T? component) where T : class
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.TryGetComponent(Uid, out component);
        }

        public T? GetComponentOrNull<T>() where T : class
        {
            return TryGetComponent(out T? component) ? component : default;
        }

        /// <inheritdoc />
        public bool TryGetComponent(Type type, [NotNullWhen(true)] out IComponent? component)
        {
            DebugTools.Assert(!Deleted, "Tried to get component on a deleted entity.");

            return EntityManager.TryGetComponent(Uid, type, out component);
        }

        public IComponent? GetComponentOrNull(Type type)
        {
            return TryGetComponent(type, out var component) ? component : null;
        }

        /// <inheritdoc />
        public void Delete()
        {
            EntityManager.DeleteEntity(this);
        }

        /// <inheritdoc />
        public IEnumerable<IComponent> GetAllComponents()
        {
            return EntityManager.GetComponents(Uid);
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAllComponents<T>()
        {
            return EntityManager.GetComponents<T>(Uid);
        }

        #endregion Components

        /// <inheritdoc />
        //public override string ToString()
        //{
        //    if (Deleted)
        //    {
        //        return $"{Name} ({Uid}, {Prototype?.ID})D";
        //    }
        //    return $"{Name} ({Uid}, {Prototype?.ID})";
        //}
    }
}

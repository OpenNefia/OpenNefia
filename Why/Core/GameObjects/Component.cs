using System;
using Why.Core.Reflection;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Utility;

namespace Why.Core.GameObjects
{
    /// <inheritdoc />
    [Reflect(false)]
    [ImplicitDataDefinitionForInheritors]
    public abstract class Component : IComponent
    {
        /// <inheritdoc />
        public IEntity Owner { get; set; } = default!;

        /// <inheritdoc />
        public EntityUid OwnerUid => Owner.Uid;

        /// <inheritdoc />
        public ComponentLifeStage LifeStage { get; private set; } = ComponentLifeStage.PreAdd;

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <summary>
        /// Increases the life stage from <see cref="ComponentLifeStage.PreAdd" /> to <see cref="ComponentLifeStage.Added" />,
        /// calling <see cref="OnAdd" />.
        /// </summary>
        internal void LifeAddToEntity()
        {
            DebugTools.Assert(LifeStage == ComponentLifeStage.PreAdd);

            LifeStage = ComponentLifeStage.Adding;
            OnAdd();

#if DEBUG
            if (LifeStage != ComponentLifeStage.Added)
            {
                DebugTools.Assert($"Component {this.GetType().Name} did not call base {nameof(OnAdd)} in derived method.");
            }
#endif
        }

        /// <summary>
        /// Increases the life stage from <see cref="ComponentLifeStage.Added" /> to <see cref="ComponentLifeStage.Initialized" />,
        /// calling <see cref="Initialize" />.
        /// </summary>
        internal void LifeInitialize()
        {
            DebugTools.Assert(LifeStage == ComponentLifeStage.Added);

            LifeStage = ComponentLifeStage.Initializing;
            Initialize();

#if DEBUG
            if (LifeStage != ComponentLifeStage.Initialized)
            {
                DebugTools.Assert($"Component {this.GetType().Name} did not call base {nameof(Initialize)} in derived method.");
            }
#endif
        }

        /// <summary>
        /// Increases the life stage from <see cref="ComponentLifeStage.Initialized" /> to
        /// <see cref="ComponentLifeStage.Running" />, calling <see cref="Startup" />.
        /// </summary>
        internal void LifeStartup()
        {
            DebugTools.Assert(LifeStage == ComponentLifeStage.Initialized);

            LifeStage = ComponentLifeStage.Starting;
            Startup();

#if DEBUG
            if (LifeStage != ComponentLifeStage.Running)
            {
                DebugTools.Assert($"Component {this.GetType().Name} did not call base {nameof(Startup)} in derived method.");
            }
#endif
        }

        /// <summary>
        /// Increases the life stage from <see cref="ComponentLifeStage.Running" /> to <see cref="ComponentLifeStage.Stopped" />,
        /// calling <see cref="Shutdown" />.
        /// </summary>
        /// <remarks>
        /// Components are allowed to remove themselves in their own Startup function.
        /// </remarks>
        internal void LifeShutdown()
        {
            // Starting allows a component to remove itself in it's own Startup function.
            DebugTools.Assert(LifeStage == ComponentLifeStage.Starting || LifeStage == ComponentLifeStage.Running);

            LifeStage = ComponentLifeStage.Stopping;
            Shutdown();

#if DEBUG
            if (LifeStage != ComponentLifeStage.Stopped)
            {
                DebugTools.Assert($"Component {this.GetType().Name} did not call base {nameof(Shutdown)} in derived method.");
            }
#endif
        }

        /// <summary>
        /// Increases the life stage from <see cref="ComponentLifeStage.Stopped" /> to <see cref="ComponentLifeStage.Deleted" />,
        /// calling <see cref="OnRemove" />.
        /// </summary>
        internal void LifeRemoveFromEntity()
        {
            // can be called at any time after PreAdd, including inside other life stage events.
            DebugTools.Assert(LifeStage != ComponentLifeStage.PreAdd);

            LifeStage = ComponentLifeStage.Removing;
            OnRemove();

#if DEBUG
            if (LifeStage != ComponentLifeStage.Deleted)
            {
                DebugTools.Assert($"Component {this.GetType().Name} did not call base {nameof(OnRemove)} in derived method.");
            }
#endif
        }

        /// <inheritdoc />
        public bool Initialized => LifeStage >= ComponentLifeStage.Initializing;

        /// <inheritdoc />
        public bool Running => ComponentLifeStage.Starting <= LifeStage && LifeStage <= ComponentLifeStage.Stopping;

        /// <inheritdoc />
        public bool Deleted => LifeStage >= ComponentLifeStage.Removing;

        private static readonly ComponentAdd CompAddInstance = new();
        private static readonly ComponentInit CompInitInstance = new();
        private static readonly ComponentStartup CompStartupInstance = new();
        private static readonly ComponentShutdown CompShutdownInstance = new();
        private static readonly ComponentRemove CompRemoveInstance = new();

        private IEventBus GetBus()
        {
            // Apparently components are being created outside of the EntityManager,
            // and the Owner is not being set correctly.
            // ReSharper disable once RedundantAssertionStatement
            DebugTools.AssertNotNull(Owner);

            return Owner.EntityManager.EventBus;
        }

        /// <summary>
        /// Called when the component gets added to an entity.
        /// </summary>
        protected virtual void OnAdd()
        {
            GetBus().RaiseComponentEvent(this, CompAddInstance);
            LifeStage = ComponentLifeStage.Added;
        }

        /// <summary>
        /// Called when all of the entity's other components have been added and are available,
        /// But are not necessarily initialized yet. DO NOT depend on the values of other components to be correct.
        /// </summary>
        protected virtual void Initialize()
        {
            GetBus().RaiseComponentEvent(this, CompInitInstance);
            LifeStage = ComponentLifeStage.Initialized;
        }

        /// <summary>
        ///     Starts up a component. This is called automatically after all components are Initialized and the entity is Initialized.
        /// </summary>
        /// <remarks>
        /// Components are allowed to remove themselves in their own Startup function.
        /// </remarks>
        protected virtual void Startup()
        {
            GetBus().RaiseComponentEvent(this, CompStartupInstance);
            LifeStage = ComponentLifeStage.Running;
        }

        /// <summary>
        ///     Shuts down the component. The is called Automatically by OnRemove.
        /// </summary>
        protected virtual void Shutdown()
        {
            GetBus().RaiseComponentEvent(this, CompShutdownInstance);
            LifeStage = ComponentLifeStage.Stopped;
        }

        /// <summary>
        /// Called when the component is removed from an entity.
        /// Shuts down the component.
        /// The component has already been marked as deleted in the component manager.
        /// </summary>
        protected virtual void OnRemove()
        {
            GetBus().RaiseComponentEvent(this, CompRemoveInstance);
            LifeStage = ComponentLifeStage.Deleted;
        }
    }

    /// <summary>
    /// The life stages of an ECS component.
    /// </summary>
    public enum ComponentLifeStage
    {
        /// <summary>
        /// The component has just been allocated.
        /// </summary>
        PreAdd = 0,

        /// <summary>
        /// Currently being added to an entity.
        /// </summary>
        Adding,

        /// <summary>
        /// Has been added to an entity.
        /// </summary>
        Added,

        /// <summary>
        /// Currently being initialized.
        /// </summary>
        Initializing,

        /// <summary>
        /// Has been initialized.
        /// </summary>
        Initialized,

        /// <summary>
        /// Currently being started up.
        /// </summary>
        Starting,

        /// <summary>
        /// Has started up.
        /// </summary>
        Running,

        /// <summary>
        /// Currently shutting down.
        /// </summary>
        Stopping,

        /// <summary>
        /// Has been shut down.
        /// </summary>
        Stopped,

        /// <summary>
        /// Currently being removed from its entity.
        /// </summary>
        Removing,

        /// <summary>
        /// Removed from its entity, and is deleted.
        /// </summary>
        Deleted,
    }

    /// <summary>
    /// The component has been added to the entity. This is the first function
    /// to be called after the component has been allocated and (optionally) deserialized.
    /// </summary>
    public class ComponentAdd : EntityEventArgs { }

    /// <summary>
    /// Raised when all of the entity's other components have been added and are available,
    /// But are not necessarily initialized yet. DO NOT depend on the values of other components to be correct.
    /// </summary>
    public class ComponentInit : EntityEventArgs { }

    /// <summary>
    /// Starts up a component. This is called automatically after all components are Initialized and the entity is Initialized.
    /// This can be called multiple times during the component's life, and at any time.
    /// </summary>
    public class ComponentStartup : EntityEventArgs { }

    /// <summary>
    /// Shuts down the component. The is called Automatically by OnRemove. This can be called multiple times during
    /// the component's life, and at any time.
    /// </summary>
    public class ComponentShutdown : EntityEventArgs { }

    /// <summary>
    /// The component has been removed from the entity. This is the last function
    /// that is called before the component is freed.
    /// </summary>
    public class ComponentRemove : EntityEventArgs { }
}

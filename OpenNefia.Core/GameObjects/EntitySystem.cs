using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Reflection;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     A subsystem that acts on all components of a type at once.
    /// </summary>
    /// <remarks>
    ///     This class is instantiated by the <c>EntitySystemManager</c>, and any IoC Dependencies will be resolved.
    /// </remarks>
    [Reflect(false), PublicAPI]
    public abstract partial class EntitySystem : IEntitySystem
    {
        [Dependency] protected readonly IEntityManager EntityManager;

        protected EntitySystem() : this(default!) { }

        protected EntitySystem(IEntityManager entityManager)
        {
            EntityManager = entityManager;
            Subs = new Subscriptions(this);
        }

        /// <inheritdoc />
        public virtual void Initialize() { }

        /// <inheritdoc />
        public virtual void Update(float frameTime) { }

        /// <inheritdoc />
        public virtual void FrameUpdate(float frameTime) { }

        /// <inheritdoc />
        public virtual void Shutdown()
        {
            ShutdownSubscriptions();
        }

        #region Event Proxy

        protected void RaiseLocalEvent<T>(T message) where T : notnull
        {
            EntityManager.EventBus.RaiseEvent(message);
        }

        protected void RaiseLocalEvent(object message)
        {
            EntityManager.EventBus.RaiseEvent(message);
        }

        protected void RaiseLocalEvent<TEvent>(EntityUid uid, TEvent args, bool broadcast = true)
            where TEvent : notnull
        {
            EntityManager.EventBus.RaiseEvent(uid, args, broadcast);
        }

        protected void RaiseLocalEvent(EntityUid uid, object args, bool broadcast = true)
        {
            EntityManager.EventBus.RaiseEvent(uid, args, broadcast);
        }

        protected void RaiseLocalEvent<TEvent>(EntityUid uid, ref TEvent args, bool broadcast = true)
            where TEvent : notnull
        {
            EntityManager.EventBus.RaiseEvent(uid, ref args, broadcast);
        }

        protected void RaiseLocalEvent(EntityUid uid, ref object args, bool broadcast = true)
        {
            EntityManager.EventBus.RaiseEvent(uid, ref args, broadcast);
        }

        #endregion

        #region Static Helpers
        /*
         NOTE: Static helpers relating to EntitySystems are here rather than in a
         static helper class for conciseness / usability. If we had an "EntitySystems" static class
         it would conflict with any imported namespace called "EntitySystems" and require using alias directive, and
         if we called it something longer like "EntitySystemUtility", writing out "EntitySystemUtility.Get" seems
         pretty tedious for a potentially commonly-used method. Putting it here allows writing "EntitySystem.Get"
         which is nice and concise.
         */

        /// <summary>
        /// Gets the indicated entity system.
        /// </summary>
        /// <typeparam name="T">entity system to get</typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : IEntitySystem
        {
            return IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<T>();
        }

        /// <summary>
        /// Tries to get an entity system of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of entity system to find.</typeparam>
        /// <param name="entitySystem">instance matching the specified type (if exists).</param>
        /// <returns>If an instance of the specified entity system type exists.</returns>
        public static bool TryGet<T>([NotNullWhen(true)] out T? entitySystem) where T : IEntitySystem
        {
            return IoCManager.Resolve<IEntitySystemManager>().TryGetEntitySystem(out entitySystem);
        }

        /// <summary>
        ///     Injects dependencies into all fields with <see cref="DependencyAttribute"/> on the provided object,
        ///     using the dependency collection of the <see cref="IEntitySystemManager"/>.
        /// </summary>
        /// <param name="obj">The object to inject into.</param>
        /// <exception cref="UnregisteredDependencyException">
        ///     Thrown if a dependency field on the object is not registered.
        /// </exception>
        public static T InjectDependencies<T>(T obj) where T : notnull
        {
            return IoCManager.Resolve<IEntitySystemManager>().InjectDependencies(obj);
        }

        #endregion
    }
}

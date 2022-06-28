using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace OpenNefia.Core.GameObjects
{
    public abstract partial class EntitySystem
    {
        private List<SubBase>? _subscriptions;

        /// <summary>
        /// A handle to allow subscription on this entity system's behalf.
        /// </summary>
        protected Subscriptions Subs { get; }

        protected void SubscribeLocalEvent<T>(
            EntityEventHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            SubEvent(handler, priority);
        }

        protected void SubscribeLocalEvent<T>(
            EntityEventRefHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            SubEvent(handler, priority);
        }

        protected void SubscribeAllEvent<T>(
            EntityEventHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            SubEvent(handler, priority);
        }

        private void SubEvent<T>(
            EntityEventHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeEvent(this, handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>());
        }

        private void SubEvent<T>(
            EntityEventRefHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeEvent(this, handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>());
        }

        protected void SubscribeLocalEvent<TComp, TEvent>(
            ComponentEventHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeLocalEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubLocal<TComp, TEvent>());
        }

        protected void SubscribeLocalEvent<TComp, TEvent>(
            ComponentEventRefHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeLocalEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubLocal<TComp, TEvent>());
        }

        private void ShutdownSubscriptions()
        {
            if (_subscriptions == null)
                return;

            foreach (var sub in _subscriptions)
            {
                sub.Unsubscribe(this, EntityManager.EventBus);
            }

            _subscriptions = null;
        }

        /// <summary>
        /// API class to allow registering on an EntitySystem's behalf.
        /// Intended to support creation of boilerplate-reduction-methods
        /// that need to subscribe stuff on an entity system.
        /// </summary>
        [PublicAPI]
        public sealed class Subscriptions
        {
            public EntitySystem System { get; }

            internal Subscriptions(EntitySystem system)
            {
                System = system;
            }

            // Intended for helper methods, so minimal API.

            public void SubEvent<T>(
                EntityEventHandler<T> handler,
                long priority = EventPriorities.Default)
                where T : notnull
            {
                System.SubEvent(handler, priority);
            }

            public void SubscribeLocalEvent<TComp, TEvent>(
                ComponentEventHandler<TComp, TEvent> handler,
                long priority = EventPriorities.Default)
                where TComp : IComponent
                where TEvent : EntityEventArgs
            {
                System.SubscribeLocalEvent(handler, priority);
            }
        }

        private abstract class SubBase
        {
            public abstract void Unsubscribe(EntitySystem sys, IEventBus bus);
        }

        private sealed class SubBroadcast<T> : SubBase where T : notnull
        {
            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeEvent<T>(sys);
            }
        }

        private sealed class SubLocal<TComp, TBase> : SubBase where TComp : IComponent where TBase : notnull
        {
            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeAllLocalEvents<TComp, TBase>();
            }
        }
    }
}

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

        protected void SubscribeBroadcast<T>(
            BroadcastEventHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            SubBroadcastEvent(handler, priority);
        }

        protected void SubscribeBroadcast<T>(
            BroadcastEventRefHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            SubBroadcastEvent(handler, priority);
        }

        private void SubBroadcastEvent<T>(
            BroadcastEventHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeBroadcastEvent(this, handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>());
        }

        private void SubBroadcastEvent<T>(
            BroadcastEventRefHandler<T> handler,
            long priority = EventPriorities.Default)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeBroadcastEvent(this, handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>());
        }

        protected void SubscribeComponent<TComp, TEvent>(
            ComponentEventHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeComponentEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubComp<TComp, TEvent>());
        }

        protected void SubscribeComponent<TComp, TEvent>(
            ComponentEventRefHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeComponentEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubEntity<TEvent>());
        }

        protected void SubscribeEntity<TEvent>(
            EntityEventHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeEntityEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubEntity<TEvent>());
        }

        protected void SubscribeEntity<TEvent>(
            EntityEventRefHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeEntityEvent(handler, priority);

            _subscriptions ??= new();
            _subscriptions.Add(new SubEntity<TEvent>());
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

            public void SubBroadcast<T>(
                BroadcastEventHandler<T> handler,
                long priority = EventPriorities.Default)
                where T : notnull
            {
                System.SubBroadcastEvent(handler, priority);
            }

            public void SubscribeLocalEvent<TComp, TEvent>(
                ComponentEventHandler<TComp, TEvent> handler,
                long priority = EventPriorities.Default)
                where TComp : IComponent
                where TEvent : EntityEventArgs
            {
                System.SubscribeComponent(handler, priority);
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

        private sealed class SubComp<TComp, TBase> : SubBase where TComp : IComponent where TBase : notnull
        {
            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeAllComponentEvents<TComp, TBase>();
            }
        }

        private sealed class SubEntity<TBase> : SubBase where TBase : notnull
        {
            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeAllEntityEvents<TBase>();
            }
        }
    }
}

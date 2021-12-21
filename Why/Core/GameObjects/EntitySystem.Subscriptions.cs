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

        protected void SubscribeNetworkEvent<T>(
            EntityEventHandler<T> handler,
            Type[]? before = null, Type[]? after = null)
            where T : notnull
        {
            SubEvent(EventSource.Network, handler, before, after);
        }

        protected void SubscribeLocalEvent<T>(
            EntityEventHandler<T> handler,
            Type[]? before = null, Type[]? after = null)
            where T : notnull
        {
            SubEvent(EventSource.Local, handler, before, after);
        }

        protected void SubscribeLocalEvent<T>(
            EntityEventRefHandler<T> handler,
            Type[]? before = null, Type[]? after = null)
            where T : notnull
        {
            SubEvent(EventSource.Local, handler, before, after);
        }

        protected void SubscribeAllEvent<T>(
            EntityEventHandler<T> handler,
            Type[]? before = null, Type[]? after = null)
            where T : notnull
        {
            SubEvent(EventSource.All, handler, before, after);
        }

        private void SubEvent<T>(
            EventSource src,
            EntityEventHandler<T> handler,
            Type[]? before, Type[]? after)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeEvent(src, this, handler, GetType(), before, after);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>(src));
        }

        private void SubEvent<T>(
            EventSource src,
            EntityEventRefHandler<T> handler,
            Type[]? before, Type[]? after)
            where T : notnull
        {
            EntityManager.EventBus.SubscribeEvent(src, this, handler, GetType(), before, after);

            _subscriptions ??= new();
            _subscriptions.Add(new SubBroadcast<T>(src));
        }

        protected void SubscribeLocalEvent<TComp, TEvent>(
            ComponentEventHandler<TComp, TEvent> handler,
            Type[]? before = null, Type[]? after = null)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeLocalEvent(handler, GetType(), before, after);

            _subscriptions ??= new();
            _subscriptions.Add(new SubLocal<TComp, TEvent>());
        }

        protected void SubscribeLocalEvent<TComp, TEvent>(
            ComponentEventRefHandler<TComp, TEvent> handler,
            Type[]? before = null, Type[]? after = null)
            where TComp : IComponent
            where TEvent : notnull
        {
            EntityManager.EventBus.SubscribeLocalEvent(handler, GetType(), before, after);

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
                EventSource src,
                EntityEventHandler<T> handler,
                Type[]? before = null, Type[]? after = null)
                where T : notnull
            {
                System.SubEvent(src, handler, before, after);
            }

            public void SubscribeLocalEvent<TComp, TEvent>(
                ComponentEventHandler<TComp, TEvent> handler,
                Type[]? before = null, Type[]? after = null)
                where TComp : IComponent
                where TEvent : EntityEventArgs
            {
                System.SubscribeLocalEvent(handler, before, after);
            }
        }

        private abstract class SubBase
        {
            public abstract void Unsubscribe(EntitySystem sys, IEventBus bus);
        }

        private sealed class SubBroadcast<T> : SubBase where T : notnull
        {
            private readonly EventSource _source;

            public SubBroadcast(EventSource source)
            {
                _source = source;
            }

            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeEvent<T>(_source, sys);
            }
        }

        private sealed class SubLocal<TComp, TBase> : SubBase where TComp : IComponent where TBase : notnull
        {
            public override void Unsubscribe(EntitySystem sys, IEventBus bus)
            {
                bus.UnsubscribeLocalEvent<TComp, TBase>();
            }
        }
    }
}

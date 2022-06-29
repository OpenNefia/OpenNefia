using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace OpenNefia.Core.GameObjects
{
    public interface IEventBus : IDirectedEventBus, IBroadcastEventBus
    {
    }

    public interface IDirectedEventBus
    {
        void RaiseEvent<TEvent>(EntityUid uid, TEvent args, bool broadcast = true)
            where TEvent : notnull;

        void RaiseEvent(EntityUid uid, object args, bool broadcast = true);

        /// <summary>
        /// Subscribes an event handler targeting entities with a given component.
        /// </summary>
        void SubscribeComponentEvent<TComp, TEvent>(
            ComponentEventHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull;

        /// <summary>
        /// Subscribes an event handler that will be run on all entities.
        /// </summary>
        void SubscribeEntityEvent<TEvent>(
            EntityEventHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull;

        #region Ref Subscriptions

        void RaiseEvent<TEvent>(EntityUid uid, ref TEvent args, bool broadcast = true)
            where TEvent : notnull;

        void RaiseEvent(EntityUid uid, ref object args, bool broadcast = true);

        /// <summary>
        /// Subscribes an event handler targeting entities with a given component, by reference.
        /// </summary>
        void SubscribeComponentEvent<TComp, TEvent>(
            ComponentEventRefHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull;

        /// <summary>
        /// Subscribes an event handler that will be run on all entities, by reference.
        /// </summary>
        void SubscribeEntityEvent<TEvent>(
            EntityEventRefHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull;

        #endregion

        void UnsubscribeAllComponentEvents<TComp, TEvent>()
            where TComp : IComponent
            where TEvent : notnull;

        void UnsubscribeAllEntityEvents<TEvent>()
            where TEvent : notnull;

        /// <summary>
        /// Dispatches an event directly to a specific component.
        /// </summary>
        /// <remarks>
        /// This has a very specific purpose, and has massive potential to be abused.
        /// DO NOT EXPOSE THIS TO CONTENT.
        /// </remarks>
        /// <typeparam name="TEvent">Event to dispatch.</typeparam>
        /// <param name="component">Component receiving the event.</param>
        /// <param name="args">Event arguments for the event.</param>
        internal void RaiseDirectedComponentEvent<TEvent>(IComponent component, TEvent args)
            where TEvent : notnull;

        /// <summary>
        /// Dispatches an event directly to a specific component, by-ref.
        /// </summary>
        /// <remarks>
        /// This has a very specific purpose, and has massive potential to be abused.
        /// DO NOT EXPOSE THIS TO CONTENT.
        /// </remarks>
        /// <typeparam name="TEvent">Event to dispatch.</typeparam>
        /// <param name="component">Component receiving the event.</param>
        /// <param name="args">Event arguments for the event.</param>
        internal void RaiseDirectedComponentEvent<TEvent>(IComponent component, ref TEvent args)
            where TEvent : notnull;
    }

    internal partial class EntityEventBus : IDirectedEventBus, IEventBus, IDisposable
    {
        private delegate void DirectedEventHandler(EntityUid uid, IComponent comp, ref Unit args);

        private delegate void DirectedEventHandler<TEvent>(EntityUid uid, IComponent comp, ref TEvent args)
            where TEvent : notnull;

        private delegate void DirectedEntityEventHandler(EntityUid uid, ref Unit args);

        private delegate void DirectedEntityEventHandler<TEvent>(EntityUid uid, ref TEvent args)
            where TEvent : notnull;

        /// <summary>
        /// Dummy component type that makes entity event handlers targeting no
        /// components work.
        /// </summary>
        private sealed class NullComponentType : IComponent
        {
            public ComponentLifeStage LifeStage => ComponentLifeStage.Deleted;
            public EntityUid Owner => EntityUid.Invalid;
            public string Name => string.Empty;
            public bool Initialized => false;
            public bool Running => false;
            public bool Deleted => false;
        }

        private IEntityManager _entMan;
        private EventTables _eventTables;
        private int _nextEventIndex = 0;

        /// <summary>
        /// Constructs a new instance of <see cref="EntityEventBus"/>.
        /// </summary>
        /// <param name="entMan">The entity manager to watch for entity/component events.</param>
        public EntityEventBus(IEntityManager entMan)
        {
            _entMan = entMan;
            _eventTables = new EventTables(_entMan);
        }

        /// <inheritdoc />
        void IDirectedEventBus.RaiseDirectedComponentEvent<TEvent>(IComponent component, TEvent args)
        {
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            _eventTables.DispatchComponent<TEvent>(component.Owner, component, ref unitRef, false);
        }

        /// <inheritdoc />
        void IDirectedEventBus.RaiseDirectedComponentEvent<TEvent>(IComponent component, ref TEvent args)
        {
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            _eventTables.DispatchComponent<TEvent>(component.Owner, component, ref unitRef, true);
        }

        /// <inheritdoc />
        public void RaiseEvent<TEvent>(EntityUid uid, TEvent args, bool broadcast = true)
            where TEvent : notnull
        {
            var type = typeof(TEvent);
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            RaiseEventCore(uid, ref unitRef, type, broadcast, false);
        }

        /// <inheritdoc />
        public void RaiseEvent(EntityUid uid, object args, bool broadcast = true)
        {
            var type = args.GetType();
            ref var unitRef = ref Unsafe.As<object, Unit>(ref args);

            RaiseEventCore(uid, ref unitRef, type, broadcast, false);
        }

        /// <inheritdoc/>
        public void RaiseEvent<TEvent>(EntityUid uid, ref TEvent args, bool broadcast = true)
            where TEvent : notnull
        {
            var type = typeof(TEvent);
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            RaiseEventCore(uid, ref unitRef, type, broadcast, true);
        }

        /// <inheritdoc/>
        public void RaiseEvent(EntityUid uid, ref object args, bool broadcast = true)
        {
            var type = args.GetType();
            ref var unitRef = ref Unsafe.As<object, Unit>(ref args);

            RaiseEventCore(uid, ref unitRef, type, broadcast, true);
        }

        private void RaiseEventCore(EntityUid uid, ref Unit unitRef, Type eventType, bool broadcast, bool byRef)
        {
            if (!_eventTables.TryGetEntityTable(uid, eventType, out var table))
            {
                // Act as if this is just a plain broadcast event.
                // This is important if no component event handlers are registered, but there are
                // still broadcast handlers.
                if (broadcast)
                    ProcessBroadcastEvent(ref unitRef, eventType, byRef);

                return;
            }

            // The entity event table weaves together the component and broadcast event handlers
            // with the same priority sorting system.
            if (table.Dirty)
            {
                var found = new List<(HandlerAndCompType, OrderingData)>();

                _eventTables.CollectOrdered(uid, eventType, found, byRef);
                CollectBroadcastOrdered(eventType, found, byRef);

                table.Set(found);
            }

            _eventTables.Dispatch(uid, eventType, ref unitRef, broadcast, byRef);
        }

        /// <inheritdoc />
        public void SubscribeComponentEvent<TComp, TEvent>(
            ComponentEventHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default)
            where TComp : IComponent
            where TEvent : notnull
        {
            void EventHandler(EntityUid uid, IComponent comp, ref TEvent args)
                => handler(uid, (TComp)comp, args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.SubscribeComponent<TEvent>(typeof(TComp), typeof(TEvent), EventHandler, orderData, false);
        }

        /// <inheritdoc/>
        public void SubscribeComponentEvent<TComp, TEvent>(ComponentEventRefHandler<TComp, TEvent> handler,
            long priority = EventPriorities.Default) where TComp : IComponent where TEvent : notnull
        {
            void EventHandler(EntityUid uid, IComponent comp, ref TEvent args)
                => handler(uid, (TComp)comp, ref args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.SubscribeComponent<TEvent>(typeof(TComp), typeof(TEvent), EventHandler, orderData, true);
        }

        /// <inheritdoc />
        public void SubscribeEntityEvent<TEvent>(
            EntityEventHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull
        {
            void EventHandler(EntityUid uid, ref TEvent args)
                => handler(uid, args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.SubscribeEntity<TEvent>(typeof(TEvent), EventHandler, orderData, false);
        }

        /// <inheritdoc/>
        public void SubscribeEntityEvent<TEvent>(EntityEventRefHandler<TEvent> handler,
            long priority = EventPriorities.Default)
            where TEvent : notnull
        {
            void EventHandler(EntityUid uid, ref TEvent args)
                => handler(uid, ref args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.SubscribeEntity<TEvent>(typeof(TEvent), EventHandler, orderData, true);
        }

        /// <inheritdoc />
        public void UnsubscribeAllComponentEvents<TComp, TEvent>()
            where TComp : IComponent
            where TEvent : notnull
        {
            _eventTables.UnsubscribeAll(typeof(TComp), typeof(TEvent));
        }

        /// <inheritdoc />
        public void UnsubscribeAllEntityEvents<TEvent>()
            where TEvent : notnull
        {
            UnsubscribeAllComponentEvents<NullComponentType, TEvent>();
        }

        // Null component type means broadcast event
        internal record HandlerAndCompType(RefEventHandler Handler, Type? ComponentType);

        private class EventTables : IDisposable
        {
            private IEntityManager _entMan;
            private IComponentFactory _comFac;

            // eUid -> EventType -> { CompType1, ... CompTypeN }
            private Dictionary<EntityUid, Dictionary<Type, EntityEventTable>> _eventTables;
            
            private readonly NullComponentType _nullComp = new();

            public class EntityEventTable
            {
                public EntityEventTable() { }

                public HashSet<Type> SubscribedComponents = new();
                public List<HandlerAndCompType> CachedSortedHandlers = new();
                public bool Dirty { get; set; } = true;

                public void AddComponentType(Type compType)
                {
                    SubscribedComponents.Add(compType);
                    Dirty = true;
                }

                public void RemoveComponentType(Type compType)
                {
                    SubscribedComponents.Remove(compType);
                    Dirty = true;
                }

                public void Set(List<(HandlerAndCompType, OrderingData)> found)
                {
                    CachedSortedHandlers = found.OrderBy(x => x.Item2).Select(x => x.Item1).ToList();
                    Dirty = false;
                }
            }

            // EventType -> CompType -> Handlers (sorted)
            private Dictionary<Type, Dictionary<Type, List<DirectedRegistration>>> _subscriptions;

            // prevents shitcode, get your subscriptions figured out before you start spawning entities
            private bool _subscriptionLock;

            public EventTables(IEntityManager entMan)
            {
                _entMan = entMan;
                _comFac = entMan.ComponentFactory;

                _entMan.EntityAdded += OnEntityAdded;
                _entMan.EntityDeleted += OnEntityDeleted;

                _entMan.ComponentAdded += OnComponentAdded;
                _entMan.ComponentRemoved += OnComponentRemoved;

                _eventTables = new();
                _subscriptions = new();
                _subscriptionLock = false;
            }

            private void OnEntityAdded(object? sender, EntityUid e)
            {
                AddEntity(e);
            }

            private void OnEntityDeleted(object? sender, EntityUid e)
            {
                RemoveEntity(e);
            }

            private void OnComponentAdded(object? sender, ComponentEventArgs e)
            {
                _subscriptionLock = true;

                AddComponent(e.Owner, e.Component.GetType());
            }

            private void OnComponentRemoved(object? sender, ComponentEventArgs e)
            {
                RemoveComponent(e.Owner, e.Component.GetType());
            }

            private void AddSubscription(Type compType, Type eventType, DirectedRegistration registration)
            {
                if (_subscriptionLock)
                    throw new InvalidOperationException("Subscription locked.");

                var referenceEvent = eventType.HasCustomAttribute<ByRefEventAttribute>();

                if (referenceEvent != registration.ReferenceEvent)
                    throw new InvalidOperationException(
                        $"Attempted to subscribe by-ref and by-value to the same directed event! comp={compType.Name}, event={eventType.Name} eventIsByRef={referenceEvent} subscriptionIsByRef={registration.ReferenceEvent}");

                if (!_subscriptions.TryGetValue(compType, out var compSubs))
                {
                    compSubs = new Dictionary<Type, List<DirectedRegistration>>();
                    _subscriptions.Add(compType, compSubs);
                }

                if (!compSubs.ContainsKey(eventType))
                    compSubs.Add(eventType, new List<DirectedRegistration>());

                var registrations = compSubs[eventType];
                registrations.Add(registration);
            }

            public void SubscribeComponent<TEvent>(Type compType, Type eventType, DirectedEventHandler<TEvent> handler,
                OrderingData order, bool byReference)
                where TEvent : notnull
            {
                AddSubscription(compType, eventType, new DirectedRegistration(handler, order,
                    (EntityUid uid, IComponent comp, ref Unit ev) =>
                    {
                        ref var tev = ref Unsafe.As<Unit, TEvent>(ref ev);
                        handler(uid, comp, ref tev);
                    }, byReference));
            }

            public void SubscribeEntity<TEvent>(Type eventType, DirectedEntityEventHandler<TEvent> handler,
                OrderingData order, bool byReference)
                where TEvent : notnull
            {
                AddSubscription(typeof(NullComponentType), eventType, new DirectedRegistration(handler, order,
                    (EntityUid uid, IComponent comp, ref Unit ev) =>
                    {
                        ref var tev = ref Unsafe.As<Unit, TEvent>(ref ev);
                        handler(uid, ref tev);
                    }, byReference));
            }

            public void UnsubscribeAll(Type compType, Type eventType)
            {
                if (_subscriptionLock)
                    throw new InvalidOperationException("Subscription locked.");

                if (!_subscriptions.TryGetValue(compType, out var compSubs))
                    return;

                compSubs.Remove(eventType);
            }

            private void AddEntity(EntityUid euid)
            {
                var eventTable = new Dictionary<Type, EntityEventTable>();
                _eventTables.Add(euid, eventTable);

                // Add entity (non-component) subs.

                if (!_subscriptions.TryGetValue(typeof(NullComponentType), out var compSubs))
                    return;

                foreach (var kvSub in compSubs)
                {
                    if (!eventTable.TryGetValue(kvSub.Key, out var entityTable))
                    {
                        entityTable = new EntityEventTable();
                        eventTable.Add(kvSub.Key, entityTable);
                    }

                    entityTable.AddComponentType(typeof(NullComponentType));
                }
            }

            private void RemoveEntity(EntityUid euid)
            {
                _eventTables.Remove(euid);
            }

            private void AddComponent(EntityUid euid, Type compType)
            {
                var eventTable = _eventTables[euid];

                var enumerator = GetReferences(compType);
                while (enumerator.MoveNext(out var compRefType))
                {
                    if (!_subscriptions.TryGetValue(compRefType, out var compSubs))
                        continue;

                    foreach (var kvSub in compSubs)
                    {
                        if (!eventTable.TryGetValue(kvSub.Key, out var entityTable))
                        {
                            entityTable = new EntityEventTable();
                            eventTable.Add(kvSub.Key, entityTable);
                        }

                        entityTable.AddComponentType(compRefType);
                    }
                }
            }

            private void RemoveComponent(EntityUid euid, Type compType)
            {
                var eventTable = _eventTables[euid];

                var enumerator = GetReferences(compType);
                while (enumerator.MoveNext(out var type))
                {
                    if (!_subscriptions.TryGetValue(type, out var compSubs))
                        continue;

                    foreach (var kvSub in compSubs)
                    {
                        if (!eventTable.TryGetValue(kvSub.Key, out var subscribedComps))
                            return;

                        subscribedComps.RemoveComponentType(type);
                    }
                }
            }

            public void Dispatch(EntityUid euid, Type eventType, ref Unit args, bool broadcast, bool dispatchByReference)
            {
                if (!TryGetSubscriptions(eventType, euid, broadcast, dispatchByReference, out var enumerator))
                    return;

                while (enumerator.MoveNext(out var handler))
                {
                    handler.Invoke(ref args);
                }
            }

            public void CollectOrdered(
                EntityUid euid,
                Type eventType,
                List<(HandlerAndCompType, OrderingData)> found,
                bool byRef)
            {
                var eventTable = _eventTables[euid];

                if (!eventTable.TryGetValue(eventType, out var entityEventTable))
                    return;

                Dictionary<Type, List<DirectedRegistration>>? compSubs;

                foreach (var compType in entityEventTable.SubscribedComponents)
                {
                    if (!_subscriptions.TryGetValue(compType, out compSubs))
                        return;

                    if (!compSubs.TryGetValue(eventType, out var registrations))
                        return;

                    foreach (var reg in registrations)
                    {
                        if (reg.ReferenceEvent != byRef)
                            ThrowByRefMisMatch(reg.ReferenceEvent);

                        IComponent component;
                        if (compType == typeof(NullComponentType))
                            component = _nullComp; // Handler ignores this component.
                        else
                            component = _entMan.GetComponent(euid, compType);

                        found.Add((new((ref Unit ev) => reg.Handler(euid, component, ref ev), compType), reg.Ordering));
                    }
                }
            }

            public void DispatchComponent<TEvent>(EntityUid euid, IComponent component, ref Unit args, bool dispatchByReference)
                where TEvent : notnull
            {
                var enumerator = GetReferences(component.GetType());
                var regs = new List<DirectedRegistration>();

                while (enumerator.MoveNext(out var type))
                {
                    if (!_subscriptions.TryGetValue(type, out var compSubs))
                        continue;

                    if (!compSubs.TryGetValue(typeof(TEvent), out var registrations))
                        continue;

                    foreach (var reg in registrations)
                    {
                        if (reg.ReferenceEvent != dispatchByReference)
                            ThrowByRefMisMatch(reg.ReferenceEvent);

                        regs.Add(reg);
                    }
                }

                // TODO: cache sorting
                regs.Sort();

                foreach (var reg in regs)
                {
                    reg.Handler(euid, component, ref args);
                }
            }

            public void ClearEntities()
            {
                _eventTables = new();
                _subscriptionLock = false;
            }

            public void Clear()
            {
                ClearEntities();
                _subscriptions = new();
            }

            public void Dispose()
            {
                _entMan.EntityAdded -= OnEntityAdded;
                _entMan.EntityDeleted -= OnEntityDeleted;

                _entMan.ComponentAdded -= OnComponentAdded;
                _entMan.ComponentRemoved -= OnComponentRemoved;

                // punishment for use-after-free
                _entMan = null!;
                _eventTables = null!;
                _subscriptions = null!;
            }

            /// <summary>
            ///     Enumerates the type's component references, returning the type itself last.
            /// </summary>
            private ReferencesEnumerator GetReferences(Type type)
            {
                return new(type, _comFac.GetRegistration(type).References);
            }

            /// <summary>
            ///     Enumerates all subscriptions for an event on a specific entity, returning the component instances and registrations.
            /// </summary>
            private bool TryGetSubscriptions(Type eventType, EntityUid euid, bool broadcast, bool byRef, [NotNullWhen(true)] out SubscriptionsEnumerator enumerator)
            {
                if (!_eventTables.TryGetValue(euid, out var eventTable))
                {
                    enumerator = default!;
                    return false;
                }

                // No subscriptions to this event type, return null.
                if (!eventTable.TryGetValue(eventType, out var entityTable))
                {
                    enumerator = default;
                    return false;
                }

                DebugTools.Assert(!entityTable.Dirty, "Events must be sorted and cached by now!");

                enumerator = new(entityTable.CachedSortedHandlers.GetEnumerator(), broadcast);
                return true;
            }

            public bool TryGetEntityTable(EntityUid euid, Type eventType, [NotNullWhen(true)] out EntityEventTable? table)
            {
                if (!_eventTables.TryGetValue(euid, out var eventTable))
                {
                    table = default!;
                    return false;
                }

                return eventTable.TryGetValue(eventType, out table);
            }

            public void SetCompTypeDirty(Type eventType)
            {
                foreach (var table in _eventTables.Values)
                {
                    if (table.TryGetValue(eventType, out var entityTable))
                    {
                        entityTable.Dirty = true;
                    }
                }
            }

            private struct ReferencesEnumerator
            {
                private readonly Type _baseType;
                private readonly IReadOnlyList<Type> _list;
                private readonly int _totalLength;
                private int _idx;

                public ReferencesEnumerator(Type baseType, IReadOnlyList<Type> list)
                {
                    _baseType = baseType;
                    _list = list;
                    _totalLength = list.Count;
                    _idx = 0;
                }

                public bool MoveNext([NotNullWhen(true)] out Type? type)
                {
                    if (_idx >= _totalLength)
                    {
                        if (_idx++ == _totalLength)
                        {
                            type = _baseType;
                            return true;
                        }

                        type = null;
                        return false;
                    }

                    type = _list[_idx++];
                    if (type == _baseType)
                        return MoveNext(out type);

                    return true;
                }
            }

            private struct SubscriptionsEnumerator : IDisposable
            {
                private List<HandlerAndCompType>.Enumerator _enumerator;
                private readonly bool _broadcast;

                public SubscriptionsEnumerator(List<HandlerAndCompType>.Enumerator enumerator, bool broadcast)
                {
                    _enumerator = enumerator;
                    _broadcast = broadcast;
                }

                public bool MoveNext(
                    [NotNullWhen(true)] out RefEventHandler? handler)
                {
                begin:
                    _enumerator.MoveNext();

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (_enumerator.Current == null)
                    {
                        handler = null;
                        return false;
                    }

                    var handlerAndCompType = _enumerator.Current;

                    if (handlerAndCompType.ComponentType == null && !_broadcast)
                        goto begin;

                    handler = handlerAndCompType.Handler;
                    return true;
                }

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }

        /// <inheritdoc />
        public void ClearEventTables()
        {
            _eventTables.Clear();
            _nextEventIndex = 0;
        }

        public void Dispose()
        {
            _eventTables.Dispose();
            _eventTables = null!;
            _entMan = null!;
        }

        private readonly struct DirectedRegistration : IComparable<DirectedRegistration>
        {
            public readonly Delegate Original;
            public readonly OrderingData Ordering;
            public readonly DirectedEventHandler Handler;
            public readonly bool ReferenceEvent;

            public DirectedRegistration(Delegate original, OrderingData ordering, DirectedEventHandler handler,
                bool referenceEvent)
            {
                Original = original;
                Ordering = ordering;
                Handler = handler;
                ReferenceEvent = referenceEvent;
            }

            public int CompareTo(DirectedRegistration other)
            {
                return Ordering.CompareTo(other.Ordering);
            }
        }
    }

    public delegate void EntityEventHandler<in TEvent>(EntityUid uid, TEvent args)
        where TEvent : notnull;

    public delegate void EntityEventRefHandler<TEvent>(EntityUid uid, ref TEvent args)
        where TEvent : notnull;

    public delegate void ComponentEventHandler<in TComp, in TEvent>(EntityUid uid, TComp component, TEvent args)
        where TComp : IComponent
        where TEvent : notnull;

    public delegate void ComponentEventRefHandler<in TComp, TEvent>(EntityUid uid, TComp component, ref TEvent args)
        where TComp : IComponent
        where TEvent : notnull;
}

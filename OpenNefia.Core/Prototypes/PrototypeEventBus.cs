using NLua;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Prototypes
{
    public interface IPrototypeEventBus : IPrototypeDirectedEventBus
    {
    }

    public interface IPrototypeDirectedEventBus
    {
        void RaiseEvent<TProto, TEvent>(TProto prototype, TEvent args)
            where TProto : IPrototype
            where TEvent : notnull;

        void RaiseEvent<TProto, TEvent>(PrototypeId<TProto> prototypeId, TEvent args)
            where TProto : class, IPrototype
            where TEvent : notnull;

        void RaiseEvent(IPrototype prototype, object args);

        void SubscribeEvent<TProto, TEvent>(
            string prototypeID,
            PrototypeEventHandler<TProto, TEvent> handler,
            long priority = EventPriorities.Default)
            where TProto : IPrototype
            where TEvent : notnull;

        #region Ref Subscriptions

        void RaiseEvent<TProto, TEvent>(TProto prototype, ref TEvent args)
            where TProto : IPrototype
            where TEvent : notnull;

        void RaiseEvent<TProto, TEvent>(PrototypeId<TProto> prototypeId, ref TEvent args)
            where TProto : class, IPrototype
            where TEvent : notnull;

        void RaiseEvent(IPrototype prototype, ref object args);

        void SubscribeEvent<TProto, TEvent>(
            string prototypeID,
            PrototypeEventRefHandler<TProto, TEvent> handler,
            long priority = EventPriorities.Default)
            where TProto : IPrototype
            where TEvent : notnull;

        #endregion

        void UnsubscribeAllEvents<TProto, TEvent>()
            where TProto : IPrototype
            where TEvent : notnull;

        bool HasEventHandlerFor<TProto, TEvent>(TProto prototype)
            where TProto : class, IPrototype
            where TEvent : notnull;

        bool HasEventHandlerFor<TProto, TEvent>(PrototypeId<TProto> prototypeId)
            where TProto : class, IPrototype
            where TEvent : notnull;
    }

    public sealed class PrototypeEventBus : IPrototypeEventBus
    {
        // Inside this class we pass a lot of things around as "ref Unit unitRef".
        // The idea behind this is to avoid using type arguments in core dispatch that only needs to pass around a ref*
        // Type arguments require the JIT to compile a new method implementation for every event type,
        // which would start to weigh a LOT.

        internal delegate void RefEventHandler(ref Unit ev);

        private delegate void DirectedEventHandler(IPrototype proto, ref Unit args);

        private delegate void DirectedEventHandler<TEvent>(IPrototype proto, ref TEvent args)
            where TEvent : notnull;

        private IPrototypeManager _protoMan;
        private EventTables _eventTables;
        private int _nextEventIndex = 0;

        public PrototypeEventBus(IPrototypeManager protoMan)
        {
            _protoMan = protoMan;
            _eventTables = new EventTables(_protoMan);
        }

        public bool HasEventHandlerFor<TProto, TEvent>(TProto prototype)
            where TProto : class, IPrototype
            where TEvent : notnull
        {
            var byRef = typeof(TEvent).HasCustomAttribute<ByRefEventAttribute>();
            return _eventTables.HasEventHandlerFor(typeof(TProto), prototype.ID, typeof(TEvent), byRef);
        }

        public bool HasEventHandlerFor<TProto, TEvent>(PrototypeId<TProto> prototypeID)
            where TProto : class, IPrototype
            where TEvent : notnull
        {
            var byRef = typeof(TEvent).HasCustomAttribute<ByRefEventAttribute>();
            return _eventTables.HasEventHandlerFor(typeof(TProto), (string)prototypeID, typeof(TEvent), byRef);
        }

        public void RaiseEvent<TProto, TEvent>(TProto prototype, TEvent args)
            where TProto : IPrototype
            where TEvent : notnull
        {
            var type = typeof(TEvent);
            var prototypeType = prototype.GetType();
            ref var protoRef = ref Unsafe.As<TProto, Unit>(ref prototype);
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            RaiseEventCore(prototypeType, prototype.ID, ref unitRef, type, false);
        }

        public void RaiseEvent<TProto, TEvent>(PrototypeId<TProto> prototypeId, TEvent args)
            where TProto : class, IPrototype
            where TEvent : notnull
        {
            RaiseEvent(_protoMan.Index(prototypeId), args);
        }

        public void RaiseEvent(IPrototype prototype, object args)
        {
            var type = args.GetType();
            var prototypeType = prototype.GetType();
            ref var protoRef = ref Unsafe.As<IPrototype, Unit>(ref prototype);
            ref var unitRef = ref Unsafe.As<object, Unit>(ref args);

            RaiseEventCore(prototypeType, prototype.ID, ref unitRef, type, false);
        }

        public void RaiseEvent<TProto, TEvent>(TProto prototype, ref TEvent args)
            where TProto : IPrototype
            where TEvent : notnull
        {
            var type = typeof(TEvent);
            var prototypeType = prototype.GetType();
            ref var protoRef = ref Unsafe.As<TProto, Unit>(ref prototype);
            ref var unitRef = ref Unsafe.As<TEvent, Unit>(ref args);

            RaiseEventCore(prototypeType, prototype.ID, ref unitRef, type, false);
        }

        public void RaiseEvent<TProto, TEvent>(PrototypeId<TProto> prototypeId, ref TEvent args)
            where TProto : class, IPrototype
            where TEvent : notnull
        {
            RaiseEvent(_protoMan.Index(prototypeId), ref args);
        }

        public void RaiseEvent(IPrototype prototype, ref object args)
        {
            var type = args.GetType();
            var prototypeType = prototype.GetType();
            ref var protoRef = ref Unsafe.As<IPrototype, Unit>(ref prototype);
            ref var unitRef = ref Unsafe.As<object, Unit>(ref args);

            RaiseEventCore(prototypeType, prototype.ID, ref unitRef, type, false);
        }

        private void RaiseEventCore(Type prototypeType, string prototypeID, ref Unit unitRef, Type eventType, bool byRef)
        {
            _eventTables.Dispatch(prototypeType, prototypeID, eventType, ref unitRef, byRef);
        }

        public void SubscribeEvent<TProto, TEvent>(string prototypeID, PrototypeEventHandler<TProto, TEvent> handler, long priority = 0)
            where TProto : IPrototype
            where TEvent : notnull
        {
            void EventHandler(IPrototype proto, ref TEvent args)
                => handler((TProto)proto, args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.Subscribe<TEvent>(typeof(TProto), prototypeID, typeof(TEvent), EventHandler, orderData, false);
        }

        public void SubscribeEvent<TProto, TEvent>(string prototypeID, PrototypeEventRefHandler<TProto, TEvent> handler, long priority = 0)
            where TProto : IPrototype
            where TEvent : notnull
        {
            void EventHandler(IPrototype proto, ref TEvent args)
                => handler((TProto)proto, ref args);

            var orderData = new OrderingData(priority, _nextEventIndex++);

            _eventTables.Subscribe<TEvent>(typeof(TProto), prototypeID, typeof(TEvent), EventHandler, orderData, true);
        }

        // for reflection use
        public void SubscribeEventValue<TProto, TEvent>(string prototypeID, PrototypeEventHandler<TProto, TEvent> handler, long priority = 0)
            where TProto : IPrototype
            where TEvent : notnull
        {
            SubscribeEvent(prototypeID, handler, priority);
        }

        // for reflection use
        public void SubscribeEventRef<TProto, TEvent>(string prototypeID, PrototypeEventRefHandler<TProto, TEvent> handler, long priority = 0)
            where TProto : IPrototype
            where TEvent : notnull
        {
            SubscribeEvent(prototypeID, handler, priority);
        }

        public void UnsubscribeAllEvents<TProto, TEvent>()
            where TProto : IPrototype
            where TEvent : notnull
        {
            _eventTables.UnsubscribeAll(typeof(TProto), typeof(TEvent));
        }

        private class EventTables : IDisposable
        {
            private IPrototypeManager _protoMan;

            // ProtoType -> EventType -> ID -> { CompType1, ... CompTypeN }
            private Dictionary<Type, Dictionary<Type, Dictionary<string, List<DirectedRegistration>>>> _subscriptions;

            private bool _dirty;
            private bool _subscriptionLock;

            public EventTables(IPrototypeManager protoMan)
            {
                _protoMan = protoMan;

                _subscriptions = new();
                _dirty = true;
                _subscriptionLock = false;
            }

            private void AddSubscription(Type protoType, string protoID, Type eventType, DirectedRegistration registration)
            {
                if (_subscriptionLock)
                    throw new InvalidOperationException("Subscription locked.");

                var referenceEvent = eventType.HasCustomAttribute<ByRefEventAttribute>();

                if (referenceEvent != registration.ReferenceEvent)
                    throw new InvalidOperationException(
                        $"Attempted to subscribe by-ref and by-value to the same directed event! comp={protoType.Name}, event={eventType.Name} eventIsByRef={referenceEvent} subscriptionIsByRef={registration.ReferenceEvent}");

                if (!_subscriptions.TryGetValue(protoType, out var protoIDSubs))
                {
                    protoIDSubs = new Dictionary<Type, Dictionary<string, List<DirectedRegistration>>>();
                    _subscriptions.Add(protoType, protoIDSubs);
                }

                if (!protoIDSubs.TryGetValue(eventType, out var protoSubs))
                {
                    protoSubs = new Dictionary<string, List<DirectedRegistration>>();
                    protoIDSubs.Add(eventType, protoSubs);
                }

                if (!protoSubs.ContainsKey(protoID))
                    protoSubs.Add(protoID, new List<DirectedRegistration>());

                var registrations = protoSubs[protoID];
                registrations.Add(registration);
                _dirty = true;
            }

            public void Subscribe<TEvent>(Type protoType, string protoID, Type eventType, DirectedEventHandler<TEvent> handler,
                OrderingData order, bool byReference)
                where TEvent : notnull
            {
                AddSubscription(protoType, protoID, eventType, new DirectedRegistration(handler, order,
                    (IPrototype proto, ref Unit ev) =>
                    {
                        ref var tev = ref Unsafe.As<Unit, TEvent>(ref ev);
                        handler(proto, ref tev);
                    }, byReference));
            }

            public void UnsubscribeAll(Type protoType, Type eventType)
            {
                if (_subscriptionLock)
                    throw new InvalidOperationException("Subscription locked.");

                if (!_subscriptions.TryGetValue(protoType, out var protoSubs))
                    return;

                protoSubs.Remove(eventType);
                _dirty = true;
            }

            private void SortEvents()
            {
                foreach (var protoSubs in _subscriptions.Values)
                {
                    foreach (var eventSubs in protoSubs.Values)
                    {
                        foreach (var registrations in eventSubs.Values)
                        {
                            registrations.Sort();
                        }
                    }
                }
            }

            public void Dispatch(Type prototypeType, string prototypeID, Type eventType, ref Unit args, bool dispatchByReference)
            {
                if (_dirty)
                {
                    SortEvents();
                    _dirty = false;
                }

                if (!TryGetRegistrations(prototypeType, prototypeID, eventType, dispatchByReference, out var registrations))
                    return;

                var proto = _protoMan.Index(prototypeType, prototypeID);

                foreach (var reg in registrations)
                {
                    reg.Handler.Invoke(proto, ref args);
                }
            }

            /// <summary>
            ///     Enumerates all subscriptions for an event on a specific entity, returning the component instances and registrations.
            /// </summary>
            private bool TryGetRegistrations(Type protoType, string protoID, Type eventType, bool byRef, [NotNullWhen(true)] out List<DirectedRegistration>? list)
            {
                if (!_subscriptions.TryGetValue(protoType, out var eventTable))
                {
                    list = default;
                    return false;
                }

                // No subscriptions to this event type, return null.
                if (!eventTable.TryGetValue(eventType, out var protoTable))
                {
                    list = default;
                    return false;
                }

                return protoTable.TryGetValue(protoID, out list);
            }

            public bool HasEventHandlerFor(Type protoType, string protoID, Type eventType, bool byRef)
            {
                return TryGetRegistrations(protoType, protoID, eventType, byRef, out _);
            }

            public void Clear()
            {
                _subscriptions = new();
                _subscriptionLock = false;
            }

            public void Dispose()
            {
                // punishment for use-after-free
                _protoMan = null!;
                _subscriptions = null!;
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
            _protoMan = null!;
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

        private const string ValueDispatchError = "Tried to dispatch a value event to a by-reference subscription.";
        private const string RefDispatchError = "Tried to dispatch a ref event to a by-value subscription.";

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowByRefMisMatch(bool subIsByRef)
        {
            if (subIsByRef)
                throw new InvalidOperationException(ValueDispatchError);
            else
                throw new InvalidOperationException(RefDispatchError);
        }

        private sealed record OrderingData(long Priority, int InsertionIndex) : IComparable<OrderingData>
        {
            public int CompareTo(OrderingData? other)
            {
                if (Priority == other?.Priority)
                    return InsertionIndex.CompareTo(other?.InsertionIndex);

                return Priority.CompareTo(other?.Priority);
            }
        }
    }

    // This is not a real type. Whenever you see a "ref Unit" it means it's a ref to *some* kind of other type.
    // It should always be cast to/from with Unsafe.As<,>
    internal readonly struct Unit
    {
    }

    public delegate void PrototypeEventHandler<in TProto, in TEvent>(TProto prototype, TEvent args)
        where TProto : IPrototype
        where TEvent : notnull;

    public delegate void PrototypeEventRefHandler<in TProto, TEvent>(TProto prototype, ref TEvent args)
        where TProto : IPrototype
        where TEvent : notnull;

    public abstract class PrototypeEventArgs { }
    
    public abstract class HandledPrototypeEventArgs : PrototypeEventArgs
    {
        /// <summary>
        ///     If this message has already been "handled" by a previous system.
        /// </summary>
        public bool Handled { get; set; }
    }

    public abstract class TurnResultPrototypeEventArgs : HandledPrototypeEventArgs
    {
        /// <summary>
        ///     Turn result of this event.
        /// </summary>
        public TurnResult TurnResult { get; set; }

        public void Handle(TurnResult turnResult)
        {
            Handled = true;
            TurnResult = turnResult;
        }
    }
}

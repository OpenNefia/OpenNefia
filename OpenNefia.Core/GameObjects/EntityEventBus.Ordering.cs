using System;
using System.Collections.Generic;
using System.Linq;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    internal partial class EntityEventBus
    {
        private void CollectBroadcastOrdered(
            Type eventType,
            List<(HandlerAndCompType, OrderingData)> found,
            bool byRef)
        {
            if (!_eventSubscriptions.TryGetValue(eventType, out var subs))
                return;

            foreach (var handler in subs)
            {
                if (handler.ReferenceEvent != byRef)
                    ThrowByRefMisMatch();

                found.Add((new(handler.Handler, null), handler.Ordering));
            }
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
}

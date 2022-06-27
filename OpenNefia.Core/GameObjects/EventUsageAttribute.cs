using JetBrains.Annotations;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Indicates what kind of entities this event should be used with.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class EventUsageAttribute : Attribute
    {
        public EventTarget Target { get; }

        public EventUsageAttribute(EventTarget target)
        {
            Target = target;
        }
    }

    public enum EventTarget
    {
        /// <summary>
        /// This event should be used with non-map/non-area entities.
        /// </summary>
        Normal,

        /// <summary>
        /// This event should be used with map entities.
        /// </summary>
        Map,

        /// <summary>
        /// This event should be used with area entities.
        /// </summary>
        Area
    }
}

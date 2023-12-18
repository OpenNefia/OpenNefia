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
        Normal = 0x1,

        /// <summary>
        /// This event should be used with map entities.
        /// </summary>
        Map = 0x2,

        /// <summary>
        /// This event should be used with area entities.
        /// </summary>
        Area = 0x4,

        /// <summary>
        /// This event should be used with quest entities.
        /// </summary>
        Quest = 0x8,

        /// <summary>
        /// This event should be used with activity entities.
        /// </summary>
        Activity = 0x10,

        /// <summary>
        /// This event should be used with encounter entities.
        /// </summary>
        Encounter = 0x20,

        /// <summary>
        /// This event should be used with weather entities.
        /// </summary>
        Weather = 0x40,

        /// <summary>
        /// This event should be used with effect entities.
        /// </summary>
        Effect = 0x80
    }
}

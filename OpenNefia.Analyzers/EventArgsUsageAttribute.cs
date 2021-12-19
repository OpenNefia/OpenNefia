using System;

namespace OpenNefia.Analyzers
{
    /// <summary>
    /// Specifies how this event should be registered using SubscribeLocalEvent in
    /// EntitySystems.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class EventArgsUsageAttribute : Attribute
    {
        public EventArgsTargets ValidOn { get; }

        public EventArgsUsageAttribute(EventArgsTargets validOn)
        {
            ValidOn = validOn;
        }
    }

    [Flags]
    public enum EventArgsTargets
    {
        ByValue = 0x0001,
        ByRef   = 0x0002
    }
}

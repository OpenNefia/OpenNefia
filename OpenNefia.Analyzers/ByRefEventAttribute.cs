using System;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Indicates that this event must be subscribed to by reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ByRefEventAttribute : Attribute
    {
    }
}
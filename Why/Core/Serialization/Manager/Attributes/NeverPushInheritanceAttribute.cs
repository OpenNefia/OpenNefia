using System;

namespace OpenNefia.Core.Serialization.Manager.Attributes
{
    // TODO Serialization: find a way to constrain this to DataField only & make exclusive w/ AlwaysPush
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class NeverPushInheritanceAttribute : Attribute
    {
    }
}

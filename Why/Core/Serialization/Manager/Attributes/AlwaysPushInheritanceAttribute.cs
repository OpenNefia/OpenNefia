using System;

namespace Why.Core.Serialization.Manager.Attributes
{
    // TODO Serialization: find a way to constrain this to DataFields only & make exclusive w/ NeverPush
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class AlwaysPushInheritanceAttribute : Attribute
    {
    }
}

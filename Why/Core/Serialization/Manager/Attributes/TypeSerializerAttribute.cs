using System;
using JetBrains.Annotations;

namespace Why.Core.Serialization.Manager.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse]
    public class TypeSerializerAttribute : Attribute
    {
    }
}

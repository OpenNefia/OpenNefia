using System;
using JetBrains.Annotations;

namespace OpenNefia.Core.Serialization.Manager.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [MeansImplicitUse]
    public class TypeSerializerAttribute : Attribute
    {
    }
}

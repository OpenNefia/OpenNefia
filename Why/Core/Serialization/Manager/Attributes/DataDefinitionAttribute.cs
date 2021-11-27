using System;
using JetBrains.Annotations;

namespace OpenNefia.Core.Serialization.Manager.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    [MeansDataDefinition]
    [MeansImplicitUse]
    public class DataDefinitionAttribute : Attribute
    {
    }
}

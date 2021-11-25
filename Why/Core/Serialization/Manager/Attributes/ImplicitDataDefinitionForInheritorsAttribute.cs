using System;

namespace Why.Core.Serialization.Manager.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ImplicitDataDefinitionForInheritorsAttribute : Attribute
    {
    }
}

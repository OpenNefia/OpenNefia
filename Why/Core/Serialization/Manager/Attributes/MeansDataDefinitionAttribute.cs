using System;
using JetBrains.Annotations;

namespace Why.Core.Serialization.Manager.Attributes
{
    [BaseTypeRequired(typeof(Attribute))]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MeansDataDefinitionAttribute : Attribute
    {
    }
}

using System;
using JetBrains.Annotations;

namespace OpenNefia.Core.Serialization.Manager.Attributes
{
    [BaseTypeRequired(typeof(Attribute))]
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MeansDataDefinitionAttribute : Attribute
    {
    }
}

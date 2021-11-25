using System;
using JetBrains.Annotations;

namespace Why.Core.Serialization.Manager.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class DataFieldAttribute : Attribute
    {
        public readonly string Tag;
        public readonly int Priority;
        public readonly bool ReadOnly;

        /// <summary>
        ///     Whether or not this field being mapped is required for the component to function.
        ///     This will not guarantee that the field is mapped when the program is run,
        ///     it is meant to be used as metadata information.
        /// </summary>
        public readonly bool Required;

        public readonly Type? CustomTypeSerializer;

        public DataFieldAttribute([NotNull] string tag, bool readOnly = false, int priority = 1, bool required = false, Type? customTypeSerializer = null)
        {
            Tag = tag;
            Priority = priority;
            ReadOnly = readOnly;
            Required = required;
            CustomTypeSerializer = customTypeSerializer;
        }
    }
}

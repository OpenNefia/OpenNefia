using System;
using JetBrains.Annotations;

namespace Why.Core.Serialization.Manager.Attributes
{
    /// <summary>
    /// Defines a field that can be (de)serialized in YAML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class DataFieldAttribute : Attribute
    {
        /// <summary>
        /// Tag name to use. If none is provided, the serializer will use the lowercased C# field name.
        /// </summary>
        public readonly string? Tag;

        /// <summary>
        /// Numeric priority controlling the order of (de)serialization.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// If true, this type can only be deserialized.
        /// </summary>
        public readonly bool ReadOnly;

        /// <summary>
        ///     Whether or not this field being mapped is required for the component to function.
        ///     This will not guarantee that the field is mapped when the program is run,
        ///     it is meant to be used as metadata information.
        /// </summary>
        public readonly bool Required;

        /// <summary>
        /// Type of an <see cref="ITypeSerializer"/> to (de)serialize this field with.
        /// </summary>
        public readonly Type? CustomTypeSerializer;

        public DataFieldAttribute(string? tag = null, bool readOnly = false, int priority = 1, bool required = false, Type? customTypeSerializer = null)
        {
            Tag = tag;
            Priority = priority;
            ReadOnly = readOnly;
            Required = required;
            CustomTypeSerializer = customTypeSerializer;
        }
    }
}

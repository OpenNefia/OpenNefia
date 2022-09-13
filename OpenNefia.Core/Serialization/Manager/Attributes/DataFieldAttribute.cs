using System;
using JetBrains.Annotations;
using OpenNefia.Analyzers;

namespace OpenNefia.Core.Serialization.Manager.Attributes
{
    /// <summary>
    /// Defines a field that can be (de)serialized in YAML.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitAssignment]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class DataFieldAttribute : DataFieldBaseAttribute
    {
        /// <summary>
        /// Tag name to use. If none is provided, the serializer will use the lowercased C# field name.
        /// </summary>
        public readonly string? Tag;

        /// <summary>
        ///     Whether or not this field being mapped is required for the component to function.
        ///     This will not guarantee that the field is mapped when the program is run,
        ///     it is meant to be used as metadata information.
        /// </summary>
        public readonly bool Required;

        /// <summary>
        /// Whether to exclude this field from being compared against during IsSameAs() calls.
        /// </summary>
        public readonly bool NoCompare;

        public DataFieldAttribute(string? tag = null, bool readOnly = false, int priority = 1, bool required = false, Type? customTypeSerializer = null, bool noCompare = false) : base(readOnly, priority, customTypeSerializer)
        {
            Tag = tag;
            Required = required;
            NoCompare = noCompare;
        }

        public override string ToString()
        {
            return $"{Tag}";
        }
    }

    public abstract class DataFieldBaseAttribute : Attribute
    {
        public readonly int Priority;
        public readonly Type? CustomTypeSerializer;
        public readonly bool ReadOnly;

        protected DataFieldBaseAttribute(bool readOnly = false, int priority = 1, Type? customTypeSerializer = null)
        {
            ReadOnly = readOnly;
            Priority = priority;
            CustomTypeSerializer = customTypeSerializer;
        }
    }
}

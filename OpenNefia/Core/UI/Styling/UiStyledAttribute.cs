using JetBrains.Annotations;
using OpenNefia.Analyzers;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitAssignment]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class UiStyledAttribute : DataFieldAttribute
    {
        public UiStyledAttribute(string? tag = null, bool readOnly = false, int priority = 1, bool required = false, Type? customTypeSerializer = null)
            : base(tag, readOnly, priority, required, customTypeSerializer)
        {
        }
    }
}
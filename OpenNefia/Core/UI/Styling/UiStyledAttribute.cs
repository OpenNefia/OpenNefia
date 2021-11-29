using OpenNefia.Analyzers;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.UI
{
    [MeansImplicitAssignment]
    public class UiStyledAttribute : DataFieldAttribute
    {
        public UiStyledAttribute(string? tag = null, bool readOnly = false, int priority = 1, bool required = false, Type? customTypeSerializer = null)
            : base(tag, readOnly, priority, required, customTypeSerializer)
        {
        }
    }
}
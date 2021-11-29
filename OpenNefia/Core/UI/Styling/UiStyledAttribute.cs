using JetBrains.Annotations;
using OpenNefia.Analyzers;

namespace OpenNefia.Core.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [MeansImplicitAssignment]
    [MeansImplicitUse(ImplicitUseKindFlags.Assign)]
    public class UiStyledAttribute : Attribute
    {
        public UiStyledAttribute(string? tag = null)
        {
            Tag = tag;
        }

        public string? Tag { get; }
    }
}
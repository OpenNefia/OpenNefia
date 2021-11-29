using JetBrains.Annotations;
using OpenNefia.Analyzers;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.UI
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class UiStyledAttribute : Attribute
    {
        public UiStyledAttribute(string? tag = null)
        {
            Tag = tag;
        }

        public string? Tag { get; }
    }
}
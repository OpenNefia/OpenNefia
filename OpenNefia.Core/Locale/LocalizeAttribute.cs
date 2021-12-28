using System;

namespace OpenNefia.Core.Locale
{
    public class LocalizeAttribute : Attribute
    {
        public string? RootLocaleKey { get; }
        public readonly bool Required;

        public LocalizeAttribute(string? rootLocaleKey = null, bool required = false)
        {
            RootLocaleKey = rootLocaleKey;
            Required = required;
        }
    }
}
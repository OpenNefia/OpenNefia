using System;

namespace OpenNefia.Core.Locale
{
    public class LocalizeAttribute : Attribute
    {
        public string? Key { get; }
        public readonly bool Required;

        public LocalizeAttribute(string? key = null, bool required = false)
        {
            Key = key;
            Required = required;
        }
    }
}
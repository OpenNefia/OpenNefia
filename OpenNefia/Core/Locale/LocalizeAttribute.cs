using System;

namespace OpenNefia.Core.Locale
{
    public class LocalizeAttribute : Attribute, ILocalizeAttribute
    {
        public string? Key { get; set; }

        public LocalizeAttribute(string? key = null)
        {
            Key = key;
        }
    }
}
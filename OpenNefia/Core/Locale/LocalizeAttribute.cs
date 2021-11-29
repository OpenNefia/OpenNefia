using System;

namespace OpenNefia.Core
{
    public class LocalizeAttribute : Attribute, ILocalizeAttribute
    {
        public string? Key { get; set; }

        public LocalizeAttribute(string? key)
        {
            Key = key;
        }
    }
}
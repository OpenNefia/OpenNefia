using System;

namespace OpenNefia.Core
{
    internal class LocalizeOptionalAttribute : Attribute, ILocalizeAttribute
    {
        public string? Key { get; set; }
    }
}
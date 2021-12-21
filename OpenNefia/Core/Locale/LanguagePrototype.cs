﻿using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Locale
{
    [Prototype("Language", -1)]
    public class LanguagePrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;
    }

    public static class LanguagePrototypeOf
    {
        public static readonly PrototypeId<LanguagePrototype> English = new("en_US");
        public static readonly PrototypeId<LanguagePrototype> Japanese = new("ja_JP");
    }
}

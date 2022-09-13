using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Locale
{
    [Prototype("Language", -1)]
    public class LanguagePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;
    }

    public static class LanguagePrototypeOf
    {
        public static readonly PrototypeId<LanguagePrototype> English = new("en_US");
        public static readonly PrototypeId<LanguagePrototype> Japanese = new("ja_JP");
        public static readonly PrototypeId<LanguagePrototype> German = new("de_DE");
    }
}

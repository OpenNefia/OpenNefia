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

    /// <summary>
    /// Language IDs should be a pair of ISO 639-1 and ISO 3166-1 codes, separated by an underscore.
    /// https://en.wikipedia.org/wiki/ISO_639-1?useskin=vector
    /// https://en.wikipedia.org/wiki/ISO_3166-1?useskin=vector
    /// </summary>
    public static class LanguagePrototypeOf
    {
        public static readonly PrototypeId<LanguagePrototype> English = new("en_US");
        public static readonly PrototypeId<LanguagePrototype> Japanese = new("ja_JP");
        public static readonly PrototypeId<LanguagePrototype> German = new("de_DE");
        public static readonly PrototypeId<LanguagePrototype> Chinese = new("zh_CN");
    }
}

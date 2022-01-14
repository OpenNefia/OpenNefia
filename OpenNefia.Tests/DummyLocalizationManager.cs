using NLua;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Tests
{
    public class DummyLocalizationManager : ILocalizationManager
    {
        public PrototypeId<LanguagePrototype> Language => LanguagePrototypeOf.English;

        public event LanguageSwitchedDelegate? OnLanguageSwitched;

        public void DoLocalize(object o, LocaleKey key)
        {
        }

        public string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return string.Empty;
        }

        public void Initialize()
        {
        }

        public bool IsFullwidth()
        {
            return false;
        }

        public void LoadContentFile(ResourcePath luaFile)
        {
        }

        public void LoadString(string luaScript)
        {
        }

        public void Resync()
        {
        }

        public void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
        }

        public bool TryGetLocalizationData(EntityUid uid, [NotNullWhen(true)] out LuaTable? table)
        {
            table = null;
            return false;
        }

        public bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            str = null;
            return false;
        }

        string ILocalizationFetcher.GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
        {
            return string.Empty;
        }

        public string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, LocaleArg[] args)
        {
            return string.Empty;
        }
    }
}
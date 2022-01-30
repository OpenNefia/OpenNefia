using NLua;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Immutable;

namespace OpenNefia.Tests
{
    public class DummyLocalizationManager : ILocalizationManager
    {
        public PrototypeId<LanguagePrototype> Language => LanguagePrototypeOf.English;

        public event LanguageSwitchedDelegate? OnLanguageSwitched;

        public void DoLocalize(object o, LocaleKey key)
        {
        }

        public EntityLocData GetEntityData(string prototypeId)
        {
            return new EntityLocData(ImmutableDictionary.Create<string, string>());
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

        public string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T: class, IPrototype
        {
            return string.Empty;
        }

        public string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, LocaleArg[] args)
        {
            return string.Empty;
        }

        public bool TryGetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T : class, IPrototype
        {
            str = null;
            return false;
        }

        public bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, LocaleArg[] args)
        {
            str = null;
            return false;
        }
    }
}
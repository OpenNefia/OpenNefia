using NLua;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Tests
{
    public class TestingLocalizationManager : ILocalizationManager
    {
        public PrototypeId<LanguagePrototype> Language => throw new NotImplementedException();

        public void DoLocalize(object o, LocaleKey key)
        {
        }

        public string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return string.Empty;
        }

        public void Initialize(PrototypeId<LanguagePrototype> language)
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
    }
}
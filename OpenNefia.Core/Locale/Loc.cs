using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Locale
{
    /// <summary>
    /// Static wrapper around <see cref="ILocalizationManager"/>
    /// </summary>
    public static class Loc
    {
        private static ILocalizationManager _localization => IoCManager.Resolve<ILocalizationManager>();

        public static PrototypeId<LanguagePrototype> Language => _localization.Language;

        public static void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            _localization.SwitchLanguage(language);
        }

        public static bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            return _localization.TryGetString(key, out str, args);
        }

        public static string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return _localization.GetString(key, args);
        }

        public static bool IsFullwidth()
        {
            return _localization.IsFullwidth();
        }

        public static string Capitalize(string text)
        {
            // TODO
            if (Language == LanguagePrototypeOf.English)
                return text.FirstCharToUpper();

            return text;
        }

        public static string? GetPrototypeStringOpt<T>(PrototypeId<T> protoId, LocaleKey keySuffix, params LocaleArg[] args)
            where T : class, IPrototype
        {
            return _localization.GetPrototypeString(protoId, keySuffix, args);
        }

        public static string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey keySuffix, params LocaleArg[] args)
            where T : class, IPrototype
        {
            return _localization.GetPrototypeString(protoId, keySuffix, args);
        }
    }
}

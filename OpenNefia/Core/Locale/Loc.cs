using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
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

        public static void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            return _localization.SwitchLanguage(language);
        }

        public static LocaleFunc<T> GetFunction<T>(LocaleKey key)
        {
            return _localization.GetFunction<T>(key);
        }

        public static LocaleFunc<T1, T2> GetFunction<T1, T2>(LocaleKey key)
        {
            return _localization.GetFunction<T1, T2>(key);
        }

        public static string GetString(LocaleKey key)
        {
            return _localization.GetString(key);
        }

        public static bool IsFullwidth()
        {
            return _localization.IsFullwidth();
        }
    }
}

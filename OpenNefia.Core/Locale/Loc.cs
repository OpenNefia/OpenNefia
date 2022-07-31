using OpenNefia.Core.GameObjects;
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

        public static bool HasString(LocaleKey key)
        {
            return _localization.HasString(key);
        }

        public static bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            return _localization.TryGetString(key, out str, args);
        }

        public static string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return _localization.GetString(key, args);
        }

        public static EntityLocData GetLocalizationData(string prototypeId)
        {
            return _localization.GetEntityData(prototypeId);
        }

        public static bool IsFullwidth()
        {
            return _localization.IsFullwidth();
        }

        public static string Space()
        {
            if (_localization.IsFullwidth())
                return "";
            return " ";
        }

        public static string Capitalize(string text)
        {
            // TODO
            if (Language == LanguagePrototypeOf.English)
                return text.FirstCharToUpper();

            return text;
        }

        public static string GetPrototypeString<T>(T proto, LocaleKey keySuffix, params LocaleArg[] args)
            where T : class, IPrototype
            => GetPrototypeString(proto.GetStrongID(), keySuffix, args);

        public static string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey keySuffix, params LocaleArg[] args)
            where T : class, IPrototype
        {
            return _localization.GetPrototypeString(protoId, keySuffix, args);
        }

        public static string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, params LocaleArg[] args)
        {
            return _localization.GetPrototypeStringRaw(prototypeType, prototypeID, keySuffix, args);
        }

        public static bool TryGetPrototypeString<T>(T proto, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T : class, IPrototype
            => TryGetPrototypeString(proto.GetStrongID(), keySuffix, out str, args);

        public static bool TryGetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T : class, IPrototype
        {
            return _localization.TryGetPrototypeString(protoId, key, out str, args);
        }

        public static bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            return _localization.TryGetPrototypeStringRaw(prototypeType, prototypeID, keySuffix, out str, args);
        }

        public static LocaleScope MakeScope(LocaleKey keyPrefix)
        {
            return new LocaleScope(_localization, keyPrefix);
        }

        /// <summary>
        /// For use with <see cref="LocalizeAttribute"/>.
        /// </summary>
        public static LocaleScope MakeScope()
        {
            return MakeScope(LocaleKey.Empty);
        }
    }
}

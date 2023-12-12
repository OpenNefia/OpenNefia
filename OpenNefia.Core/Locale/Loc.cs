using NLua;
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

        /// <summary>
        /// Returns true if this key exists in any form in the localization environment (string, list, function, etc.)
        /// </summary>
        public static bool KeyExists(LocaleKey key)
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

        public static bool TryGetList(LocaleKey key, [NotNullWhen(true)] out IReadOnlyList<string>? list, params LocaleArg[] args)
        {
            return _localization.TryGetList(key, out list, args);
        }

        public static IReadOnlyList<string> GetList(LocaleKey key, params LocaleArg[] args)
        {
            return _localization.GetList(key, args);
        }

        public static string FormatRaw(object? obj, params LocaleArg[] args)
        {
            return _localization.FormatRaw(obj, args);
        }

        public static bool TryGetLocalizationData(EntityUid uid, [NotNullWhen(true)] out LuaTable? table)
        {
            return _localization.TryGetLocalizationData(uid, out table);
        }

        public static EntityLocData GetLocalizationData(string prototypeId)
        {
            return _localization.GetEntityData(prototypeId);
        }

        public static bool IsFullwidth()
        {
            return _localization.IsFullwidth();
        }

        /// <summary>
        /// Returns the localized whitespace character for the current language.
        /// </summary>
        public static string Space 
        {
            get
            {
                if (_localization.IsFullwidth())
                    return "";
                return " ";
            }
        }

        public static string Capitalize(string text)
        {
            // TODO
            if (Language == LanguagePrototypeOf.English)
                return text.FirstCharToUpper();

            return text;
        }

        public static bool TryGetTable(LocaleKey key, [NotNullWhen(true)] out LuaTable? table)
        {
            return _localization.TryGetTable(key, out table);
        }

        public static LuaTable GetTable(LocaleKey key)
        {
            return _localization.GetTable(key);
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

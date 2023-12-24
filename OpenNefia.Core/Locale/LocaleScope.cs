using NLua;
using OpenNefia.Core.Prototypes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Locale
{
    /// <summary>
    /// <para>
    /// Lets you shorten the namespacing for a group of locale keys.
    /// </para>
    /// <para>
    /// For example:
    /// </para>
    /// <code>
    /// var str1 = Loc.GetString("Elona.UI.Layer.TitleScreen.Window.Title");
    /// 
    /// var scope = new LocaleScope("Elona.UI.Layer.TitleScreen");
    /// var str2 = scope.GetString("Window.Title");
    /// 
    /// Assert.That(str1, Is.EqualTo(str2));
    /// </code>
    /// </summary>
    public class LocaleScope : ILocalizationFetcher, ILocalizable
    {
        private readonly ILocalizationManager _localizationManager;

        public LocaleKey KeyPrefix { get; private set; }

        public bool IsLocalized { get; private set; } = false;

        public LocaleScope(ILocalizationManager localizationManager)
            : this(localizationManager, LocaleKey.Empty)
        {
        }

        public LocaleScope(ILocalizationManager localizationManager, LocaleKey keyPrefix)
        {
            _localizationManager = localizationManager;
            KeyPrefix = keyPrefix;
        }

        public bool KeyExists(LocaleKey key)
        {
            return _localizationManager.KeyExists(KeyPrefix.With(key));
        }

        public bool PrototypeKeyExists<T>(PrototypeId<T> protoID, LocaleKey key) where T : class, IPrototype
        {
            return false;
        }

        public bool PrototypeKeyExists<T>(T proto, LocaleKey key) where T : class, IPrototype
        {
            return false;
        }

        public string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return _localizationManager.GetString(KeyPrefix.With(key), args);
        }

        public bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            return _localizationManager.TryGetString(KeyPrefix.With(key), out str, args);
        }

        public string FormatRaw(object? obj, LocaleArg[] args)
        {
            return _localizationManager.FormatRaw(obj, args);
        }

        public string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T : class, IPrototype
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
            return _localizationManager.TryGetPrototypeString(protoId, key, out str, args);
        }

        public bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, LocaleArg[] args)
        {
            return _localizationManager.TryGetPrototypeStringRaw(prototypeType, prototypeID, keySuffix, out str, args);
        }

        public bool TryGetPrototypeList<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out IReadOnlyList<string>? list, params LocaleArg[] args)
            where T: class, IPrototype
        {
            list = null;
            return false;
        }

        public bool TryGetPrototypeListRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out IReadOnlyList<string>? list, params LocaleArg[] args)
        {
            list = null;
            return false;
        }

        public bool TryGetTable(LocaleKey key, [NotNullWhen(true)] out LuaTable? table)
        {
            return _localizationManager.TryGetTable(KeyPrefix.With(key), out table);
        }

        public LuaTable GetTable(LocaleKey key)
        {
            return _localizationManager.GetTable(KeyPrefix.With(key));
        }

        public void Localize(LocaleKey key)
        {
            KeyPrefix = key.GetParent();
            IsLocalized = true;
        }

        public bool TryGetList(LocaleKey key, [NotNullWhen(true)] out IReadOnlyList<string>? list, params LocaleArg[] args)
        {
            return _localizationManager.TryGetList(KeyPrefix.With(key), out list, args);
        }

        public IReadOnlyList<string> GetList(LocaleKey key, params LocaleArg[] args)
        {
            return _localizationManager.GetList(KeyPrefix.With(key), args);
        }
    }
}
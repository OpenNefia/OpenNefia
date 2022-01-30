using OpenNefia.Core.Prototypes;
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

        public string GetString(LocaleKey key, params LocaleArg[] args)
        {
            return _localizationManager.GetString(KeyPrefix.With(key), args);
        }

        public bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            return _localizationManager.TryGetString(KeyPrefix.With(key), out str, args);
        }

        public string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T : class, IPrototype
        {
            return _localizationManager.GetPrototypeString(protoId, KeyPrefix.With(key), args);
        }

        public string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, LocaleArg[] args)
        {
            return _localizationManager.GetPrototypeStringRaw(prototypeType, prototypeID, keySuffix, args);
        }

        public bool TryGetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T: class, IPrototype
        {
            return _localizationManager.TryGetPrototypeString(protoId, key, out str, args);
        }

        public bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, LocaleArg[] args)
        {
            return _localizationManager.TryGetPrototypeStringRaw(prototypeType, prototypeID, keySuffix, out str, args);
        }

        public void Localize(LocaleKey key)
        {
            KeyPrefix = key.GetParent();
            IsLocalized = true;
        }
    }
}
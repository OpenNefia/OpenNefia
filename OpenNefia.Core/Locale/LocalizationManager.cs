using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using System.Reflection;
using NLua;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ContentPack;
using System.Buffers;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Graphics;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Configuration;
using System.Collections.Immutable;
using System.Collections.Concurrent;

namespace OpenNefia.Core.Locale
{
    public interface ILocalizationFetcher
    {
        bool HasString(LocaleKey key);
        bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args);
        string GetString(LocaleKey key, params LocaleArg[] args);
        string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T: class, IPrototype;
        string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, LocaleArg[] args);
        bool TryGetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T : class, IPrototype;
        bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, params LocaleArg[] args);
        bool TryGetTable(LocaleKey key, [NotNullWhen(true)] out LuaTable? table);
    }

    public delegate void LanguageSwitchedDelegate(PrototypeId<LanguagePrototype> newLanguage);

    public interface ILocalizationManager : ILocalizationFetcher
    {
        PrototypeId<LanguagePrototype> Language { get; }

        void Initialize();

        bool IsFullwidth();
        void SwitchLanguage(PrototypeId<LanguagePrototype> language);

        void DoLocalize(object o, LocaleKey key);

        void LoadContentFile(ResourcePath luaFile);
        void LoadString(string luaScript);
        void Resync();

        bool TryGetLocalizationData(EntityUid uid, [NotNullWhen(true)] out LuaTable? table);
        EntityLocData GetEntityData(string prototypeId);

        event LanguageSwitchedDelegate? OnLanguageSwitched;
    }

    /// <summary>
    /// An argument for localizing text using a Lua function.
    /// </summary>
    public struct LocaleArg
    {
        /// <summary>
        /// Name of this argument in the Lua source.
        /// </summary>
        /// <remarks>
        /// NOTE: This is purely for documentation purposes.
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        /// Value of this argument to pass to a Lua function.
        /// </summary>
        public object? Value { get; set; }

        public LocaleArg(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public static implicit operator LocaleArg((string, object?) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }
    }

    public partial class LocalizationManager : ILocalizationManager
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IComponentLocalizer _componentLocalizer = default!;

        public event LanguageSwitchedDelegate? OnLanguageSwitched;

        private readonly ResourcePath LocalePath = new ResourcePath("/Locale");

        private const string LocDataKey = "_LocData";
        private ConcurrentDictionary<string, EntityLocData> _entityCache = new();

        public PrototypeId<LanguagePrototype> Language { get; private set; } = LanguagePrototypeOf.English;

        public void Initialize()
        {
            _lua = CreateLuaEnv();
            ScanBuiltInFunctions();

            _config.OnValueChanged(CVars.LanguageLanguage, OnConfigLanguageChanged, true);

            _graphics.OnWindowFocused += WindowFocusedChanged;

            WatchResources();
        }

        private void OnConfigLanguageChanged(string rawID)
        {
            PrototypeId<LanguagePrototype> protoId = new(rawID);

            // This makes the test suite have a hard dependency on Core prototypes.
            /*
            if (!_protos.HasIndex(protoId))
            {
                protoId = LanguagePrototypeOf.English;
                Logger.WarningS("loc", $"No language with ID '{rawID}' registered; falling back to {protoId}");
            }
            */

            SwitchLanguage(protoId);
        }
        
        public void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            using var profiler = new ProfilerLogger(LogLevel.Info, "loc", $"Switching language to {language}");

            Language = language;
            SetLanguage(language);
            LoadAll(language, LocalePath);
            Resync();

            foreach (var layer in _uiManager.ActiveLayers)
            {
                layer.Localize();
            }

            foreach (var uid in _entityManager.GetEntityUids())
            {
                _componentLocalizer.LocalizeComponents(uid);
            }
            _entityCache.Clear();

            OnLanguageSwitched?.Invoke(language);
        }

        public bool HasString(LocaleKey key)
        {
            return (_stringStore.ContainsKey(key) || _functionStore.ContainsKey(key) || _listStore.ContainsKey(key));
        }

        public bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            static string CallFunction(LocaleKey key, LocaleArg[] args, LuaFunction func)
            {
                string? str;
                var shared = ArrayPool<object?>.Shared;
                var rented = shared.Rent(args.Length);

                for (int i = 0; i < args.Length; i++)
                    rented[i] = args[i].Value;

                try
                {
                    var result = func.Call(rented).FirstOrDefault();
                    str = $"{result ?? "null"}";
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("loc", ex, $"Error in locale function: {ex}");
                    str = $"<exception: {ex.Message} ({key})>";
                }

                shared.Return(rented);
                return str;
            }

            if (_stringStore.TryGetValue(key, out str))
            {
                return true;
            }

            if (_functionStore.TryGetValue(key, out var func))
            {
                str = CallFunction(key, args, func);

                return true;
            }

            if (_listStore.TryGetValue(key, out var list))
            {
                // This is meant to emulate the `txt` function in the HSP source.
                var obj = _random.Pick(list);

                if (obj is LuaFunction func2)
                {
                    str = CallFunction(key, args, func2);
                }
                else
                {
                    str = obj.ToString()!;
                }

                return true;
            }

            return false;
        }

        public string GetString(LocaleKey key, params LocaleArg[] args)
        {
            if (TryGetString(key, out var str, args))
                return str;

            return $"<Missing key: {key}>";
        }

        public string GetStringOrEmpty(LocaleKey key, params LocaleArg[] args)
        {
            if (TryGetString(key, out var str, args))
                return str;

            return string.Empty;
        }

        public bool TryGetTable(LocaleKey key, [NotNullWhen(true)] out LuaTable? table)
        {
            table = _lua.GetTable($"_Collected." + key);
            return table != null;
        }

        public string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T : class, IPrototype
            => GetPrototypeStringRaw(typeof(T), (string)protoId, key, args);

        public string GetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, LocaleArg[] args)
        {
            var protoTypeId = prototypeType.GetCustomAttribute<PrototypeAttribute>()!.Type;
            return GetString(new LocaleKey($"OpenNefia.Prototypes.{protoTypeId}.{prototypeID}").With(keySuffix), args);
        }

        public bool TryGetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
            where T : class, IPrototype
            => TryGetPrototypeStringRaw(typeof(T), (string)protoId, key, out str, args);

        public bool TryGetPrototypeStringRaw(Type prototypeType, string prototypeID, LocaleKey keySuffix, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            var protoTypeId = prototypeType.GetCustomAttribute<PrototypeAttribute>()!.Type;
            return TryGetString(new LocaleKey($"OpenNefia.Prototypes.{protoTypeId}.{prototypeID}").With(keySuffix), out str, args);
        }

        public bool IsFullwidth()
        {
            return Language == LanguagePrototypeOf.Japanese;
        }

        public bool TryGetLocalizationData(EntityUid uid, [NotNullWhen(true)] out LuaTable? table)
        {
            if (!_entityManager.TryGetComponent(uid, out MetaDataComponent? metadata)
                || metadata.EntityPrototype == null)
            {
                table = null;
                return false;
            }

            return TryGetTable($"OpenNefia.Prototypes.Entity.{metadata.EntityPrototype.ID}", out table);
        }

        private EntityLocData ReadEntityLocDataFromLua(string id)
        {
            if (!TryGetTable($"OpenNefia.Prototypes.Entity.{id}", out var entityTable)
                || !entityTable.TryGetTable(LocDataKey, out var locTable))
                return new(ImmutableDictionary.Create<string, string>());

            var builder = ImmutableDictionary.CreateBuilder<string, string>();
            foreach (KeyValuePair<object, object> locData in locTable)
                builder.Add(locData.Key.ToString()!, locData.Value.ToString()!);

            return new(builder.ToImmutable());
        }

        public EntityLocData GetEntityData(string prototypeId)
        {
            return _entityCache.GetOrAdd(prototypeId ?? string.Empty, (id, t) => t.ReadEntityLocDataFromLua(id), this);
        }
    }
}

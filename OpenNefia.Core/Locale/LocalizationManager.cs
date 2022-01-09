﻿using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System.Reflection;
using NLua;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.UI.Layer;
using System.Buffers;
using OpenNefia.Core.Random;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Graphics;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Timing;
using Microsoft.CodeAnalysis;

namespace OpenNefia.Core.Locale
{
    public interface ILocalizationFetcher
    {
        bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args);
        string GetString(LocaleKey key, params LocaleArg[] args);
        string GetPrototypeString<T>(PrototypeId<T> protoId, LocaleKey key, params LocaleArg[] args)
            where T: class, IPrototype;
    }

    public interface ILocalizationManager : ILocalizationFetcher
    {
        PrototypeId<LanguagePrototype> Language { get; }

        void Initialize(PrototypeId<LanguagePrototype> language);

        bool IsFullwidth();
        void SwitchLanguage(PrototypeId<LanguagePrototype> language);

        void DoLocalize(object o, LocaleKey key);

        void LoadContentFile(ResourcePath luaFile);
        void LoadString(string luaScript);
        void Resync();

        bool TryGetLocalizationData(EntityUid uid, [NotNullWhen(true)] out LuaTable? table);
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
        [Dependency] private readonly IEntityFactory _entityFactory = default!;

        private readonly ResourcePath LocalePath = new ResourcePath("/Locale");
        
        public void Initialize(PrototypeId<LanguagePrototype> language)
        {
            _lua = CreateLuaEnv();
            ScanBuiltInFunctions();

            SwitchLanguage(language);

            _graphics.OnWindowFocused += WindowFocusedChanged;

            WatchResources();
        }

        public PrototypeId<LanguagePrototype> Language { get; private set; } = LanguagePrototypeOf.English;
        
        public void SwitchLanguage(PrototypeId<LanguagePrototype> language)
        {
            using var profiler = new ProfilerLogger(LogLevel.Info, "loc", $"Switching language to {language}");

            Language = language;
            SetLanguage(language);
            LoadAll(language, LocalePath);
            Resync();

            foreach (var layer in _uiManager.ActiveLayers)
            {
                layer.Localize(layer.GetType()!.FullName!);
            }

            foreach (var uid in _entityManager.GetEntityUids())
            {
                _entityFactory.LocalizeComponents(uid);
            }
        }

        public bool TryGetString(LocaleKey key, [NotNullWhen(true)] out string? str, params LocaleArg[] args)
        {
            str = null;

            if (_stringStore.TryGetValue(key, out str))
            {
                return true;
            }

            if (_functionStore.TryGetValue(key, out var func))
            {
                var shared = ArrayPool<object?>.Shared;
                var rented = shared.Rent(args.Length);

                for (int i = 0; i < args.Length; i++)
                    rented[i] = args[i].Value;

                try
                {
                    var result = func.Call(rented)[0];
                    str = $"{result}";
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("loc", ex, $"Error in locale function: {ex}");
                    str = $"<exception: {ex.Message} ({key})>";
                }

                shared.Return(rented);

                return true;
            }

            if (_listStore.TryGetValue(key, out var list))
            {
                // This is meant to emulate the `txt` function in the HSP source.
                str = _random.Pick(list);
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
        {
            var protoTypeId = typeof(T).GetCustomAttribute<PrototypeAttribute>()!.Type;
            return GetString(new LocaleKey($"OpenNefia.Prototypes.{protoTypeId}.{protoId}").With(key), args);
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

            // TODO: put this under prototypes like everything else
            return TryGetTable($"OpenNefia.Entities.{metadata.EntityPrototype.ID}", out table);
        }
    }
}

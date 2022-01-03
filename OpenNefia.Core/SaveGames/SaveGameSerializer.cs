using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.SaveGames
{
    public delegate void GameSavedDelegate(ISaveGameHandle save);
    public delegate void GameLoadedDelegate(ISaveGameHandle save);

    /// <summary>
    /// Handles converting the serializable data of the current game state
    /// into a loadable format. This wraps <see cref="ISaveGameManager"/> by adding
    /// game save format support.
    /// </summary>
    public interface ISaveGameSerializer
    {
        event GameSavedDelegate OnGameSaved;
        event GameLoadedDelegate OnGameLoaded;

        /// <summary>
        /// Saves the global game state to the currently active save.
        /// </summary>
        void SaveGlobalData(ISaveGameHandle save);

        /// <summary>
        /// Loads the global game state from the currently active save.
        /// </summary>
        void LoadGlobalData(ISaveGameHandle save);

        /// <summary>
        /// Registers a reference to global save data that should be tracked by the
        /// save game serializer. When the game is loaded, the reference will be automatically
        /// populated from the save file.
        /// </summary>
        /// <param name="key">Globally unique key to identify this save data.</param>
        /// <param name="dataReference">Reference to the data.</param>
        void RegisterSaveData<T>(string key, T dataReference) where T : class;

        /// <summary>
        /// Registers a reference to global save data that should be tracked by the
        /// save game serializer. When the game is loaded, the reference will be automatically
        /// populated from the save file.
        /// </summary>
        /// <param name="key">Globally unique key to identify this save data.</param>
        /// <param name="ty">Type of the data. <paramref name="dataReference"/> must be assignable to this type.</param>
        /// <param name="dataReference">Reference to the data.</param>
        void RegisterSaveData(string key, Type ty, object dataReference);
    }

    public interface ISaveGameSerializerInternal : ISaveGameSerializer
    {
        void Initialize();
    }

    public class SaveGameSerializer : ISaveGameSerializerInternal
    {
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        public const string SawmillName = "save.game";

        public event GameSavedDelegate? OnGameSaved;
        public event GameLoadedDelegate? OnGameLoaded;

        private class SaveDataRegistration
        {
            public Type Type { get; }
            public object Data { get; }

            public SaveDataRegistration(Type type, object data)
            {
                Type = type;
                Data = data;
            }
        }

        private Dictionary<string, SaveDataRegistration> _trackedSaveData = new();

        public void Initialize()
        {
            foreach (var type in _entitySystemManager.SystemTypes)
            {
                if (_entitySystemManager.TryGetEntitySystem(type, out var sys))
                {
                    foreach (var prop in sys.GetType().GetProperties())
                    {
                        if (prop.TryGetCustomAttribute(out RegisterSaveDataAttribute? attr))
                        {
                            var value = prop.GetValue(sys);
                            if (value == null)
                                throw new InvalidDataException($"Expected non-nullable reference for save data '{attr.Key}', got null.");

                            RegisterSaveData(attr.Key, prop.PropertyType, value);
                        }
                    }
                }
            }
        }

        public void RegisterSaveData<T>(string key, T dataReference) where T : class
        {
            RegisterSaveData(key, typeof(T), dataReference);
        }

        public void RegisterSaveData(string key, Type ty, object dataReference)
        {
            if (!dataReference.GetType().IsAssignableTo(ty))
            {
                throw new ArgumentException($"Expected save data of type '{ty}', got '{dataReference.GetType()}'", nameof(dataReference));
            }
            if (_trackedSaveData.ContainsKey(key))
            {
                throw new ArgumentException($"Save data reference '{key}' was already registered (type: {ty})", nameof(key));
            }
            if (!_serializationManager.HasDataDefinition(ty))
            {
                throw new ArgumentException($"Type '{ty}' does not have a data definition.", nameof(ty));
            }

            Logger.DebugS(SawmillName, $"Registering global save data: {key} ({ty})");
            _trackedSaveData.Add(key, new SaveDataRegistration(ty, dataReference));
        }

        public void SaveGlobalData(ISaveGameHandle save)
        {
            // Save all the global data not tied to maps.
            var mapping = new MappingDataNode();

            foreach (var (key, reg) in _trackedSaveData)
            {
                Logger.DebugS(SawmillName, $"Saving global data: {key} ({reg.Type})");

                var node = _serializationManager.WriteValue(reg.Type, reg.Data, alwaysWrite: true);
                mapping.Add(key, node);
            }

            var global = new ResourcePath("/global.yml");
            var root = new MappingDataNode();
            root.Add("data", mapping);

            save.Files.WriteAllYaml(global, root.ToYaml());

            OnGameSaved?.Invoke(save);
        }

        public void LoadGlobalData(ISaveGameHandle save)
        {
            var global = new ResourcePath("/global.yml");
            var stream = save.Files.ReadAllYaml(global);
            var node = stream.Documents[0].RootNode.ToDataNodeCast<MappingDataNode>();

            var data = (MappingDataNode)node.Get("data");

            foreach (var (keyNode, rawNode) in data.Children)
            {
                var key = ((ValueDataNode)keyNode).Value;

                if (!_trackedSaveData.TryGetValue(key, out var reg))
                {
                    Logger.WarningS(SawmillName, $"Found global save data with key '{key}', but it wasn't registered with the save game serializer.");
                    continue;
                }

                Logger.DebugS(SawmillName, $"Loading global data: {key} ({reg.Type})");

                var raw = _serializationManager.ReadValue(reg.Type, rawNode, skipHook: true);
                _serializationManager.Copy(raw, reg.Data, skipHook: false);
            }

            OnGameLoaded?.Invoke(save);
        }
    }
}

using OpenNefia.Core.Areas;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.SaveGames
{
    public delegate void GameSavedDelegate(ISaveGameHandle save);
    public delegate void GameLoadedDelegate(ISaveGameHandle save);
    public delegate void SaveDataInitializeDelegate();

    /// <summary>
    /// Handles converting the serializable data of the current game state
    /// into a loadable format. This wraps <see cref="ISaveGameManager"/> by adding
    /// game save format support.
    /// </summary>
    public interface ISaveGameSerializer
    {
        event GameSavedDelegate BeforeGameSaved;
        event GameLoadedDelegate OnGameLoaded;
        event SaveDataInitializeDelegate OnSaveDataInitialize;

        ISaveGameHandle InitializeSaveGame(string name);

        /// <summary>
        /// Saves the game state to the provided save.
        /// </summary>
        void SaveGame(ISaveGameHandle save);

        /// <summary>
        /// Loads the game state from the provided save.
        /// </summary>
        void LoadGame(ISaveGameHandle save);

        /// <summary>
        /// Resets all entities, maps and areas, allowing a new save to load.
        /// </summary>
        void ResetGameState();
    }

    internal interface ISaveGameSerializerInternal : ISaveGameSerializer
    {
        void Initialize();

        void SaveGlobalData(ISaveGameHandle save);
        void LoadGlobalData(ISaveGameHandle save);
    }

    /// <summary>
    /// Global, engine-internal data that isn't part of either a map or an entity system.
    /// These are spread across various IoC dependencies and manually set up on save/load.
    /// </summary>
    [DataDefinition]
    internal class SessionData
    {
        /// <summary>
        /// Current player at the time of saving.
        /// </summary>
        /// <see cref="IGameSessionManager"/>
        // TODO: no EntityUid serializer...
        [DataField(required: true)]
        public int PlayerUid { get; set; }

        /// <summary>
        /// Current map at the time of saving.
        /// </summary>
        /// <see cref="IMapManager"/>
        [DataField(required: true)]
        public int ActiveMapId { get; set; }

        /// <summary>
        /// Next free entity UID at the time of saving.
        /// </summary>
        /// <see cref="IEntityManager"/>
        [DataField(required: true)]
        public int NextEntityUid { get; set; }

        /// <summary>
        /// Next free map ID at the time of saving.
        /// </summary>
        /// <see cref="IMapManagerInternal"/>
        [DataField(required: true)]
        public int NextMapId { get; set; }

        /// <summary>
        /// Next free area ID at the time of saving.
        /// </summary>
        /// <see cref="IAreaManagerInternal"/>
        [DataField(required: true)]
        public int NextAreaId { get; set; }

        /// <summary>
        /// All areas loaded at the time of saving.
        /// </summary>
        /// <see cref="IAreaManagerInternal"/>
        [DataField(required: true)]
        public Dictionary<AreaId, Area> Areas { get; set; } = new();
    }

    public sealed class SaveGameSerializer : ISaveGameSerializerInternal
    {
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IMapManagerInternal _mapManager = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IAreaManagerInternal _areaManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSessionManager = default!;
        [Dependency] private readonly IModLoader _modLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        public const string SawmillName = "save.game";

        public event GameSavedDelegate? BeforeGameSaved;
        public event GameLoadedDelegate? OnGameLoaded;
        public event SaveDataInitializeDelegate? OnSaveDataInitialize;

        private class SaveDataRegistration
        {
            public AbstractFieldInfo FieldInfo { get; }
            public object Parent { get; }

            public Type Type => FieldInfo.FieldType;

            public SaveDataRegistration(AbstractFieldInfo propertyInfo, object parent)
            {
                FieldInfo = propertyInfo;
                Parent = parent;
            }

            public void SetValueOnParent(object? value)
            {
                if (FieldInfo.TryGetBackingField(out var backingField))
                {
                    backingField.SetValue(Parent, value);
                }
                else
                {
                    FieldInfo.SetValue(Parent, value);
                }
            }
        }

        private Dictionary<string, SaveDataRegistration> _trackedSaveData = new();

        public void Initialize()
        {
            foreach (var type in _entitySystemManager.SystemTypes)
            {
                if (_entitySystemManager.TryGetEntitySystem(type, out var sys))
                {
                    RegisterSaveDataFromFields(sys);
                }
            }

            foreach (var instance in IoCManager.Instance!.Services.Values)
            {
                RegisterSaveDataFromFields(instance);
            }
        }

        private void RegisterSaveDataFromFields(object instance)
        {
            foreach (var info in instance.GetType().GetAllPropertiesAndFields())
            {
                if (info.TryGetAttribute(out RegisterSaveDataAttribute? attr))
                {
                    var value = info.GetValue(instance);
                    if (value == null)
                        throw new InvalidDataException($"Expected non-nullable reference for save data '{attr.Key}', got null.");

                    RegisterSaveData(attr.Key, info, instance);
                }
            }
        }

        private void RegisterSaveData(string key, AbstractFieldInfo field, object parent)
        {
            var ty = field.FieldType;

            if (field is SpecificPropertyInfo propertyInfo)
            {
                if (propertyInfo.PropertyInfo.GetMethod == null)
                {
                    throw new ArgumentException($"Property {propertyInfo} is annotated with {nameof(RegisterSaveDataAttribute)} but has no getter");
                }
                else if (propertyInfo.PropertyInfo.SetMethod == null)
                {
                    if (!propertyInfo.HasBackingField())
                    {
                        throw new ArgumentException($"Property {propertyInfo} in type {propertyInfo.DeclaringType} is annotated with {nameof(RegisterSaveDataAttribute)} as non-readonly but has no auto-setter");
                    }
                }
            }
            if (_trackedSaveData.ContainsKey(key))
            {
                throw new ArgumentException($"Save data reference '{key}' was already registered (type: {ty})", nameof(key));
            }
            if (!_serializationManager.CanSerializeType(ty))
            {
                throw new ArgumentException($"Type '{ty}' cannot be serialized.", nameof(ty));
            }

            Logger.DebugS(SawmillName, $"Registering global save data: {key} ({ty})");
            _trackedSaveData.Add(key, new SaveDataRegistration(field, parent));
        }

        private void ResetGlobalSaveData()
        {
            foreach (var (_, reg) in _trackedSaveData)
            {
                var newValue = Activator.CreateInstance(reg.FieldInfo.FieldType);
                reg.SetValueOnParent(newValue);
            }

            OnSaveDataInitialize?.Invoke();
        }

        private SaveGameHeader MakeSaveGameHeader(string name)
        {
            var assemblyVersions = new List<ModMetadata>();

            foreach (var mod in _modLoader.LoadedMods)
            {
                var meta = new ModMetadata()
                {
                    ID = mod.Manifest.ID,
                    Version = mod.Manifest.Version
                };
                assemblyVersions.Add(meta);
            }

            return new SaveGameHeader(name, assemblyVersions);
        }

        public ISaveGameHandle InitializeSaveGame(string name)
        {
            if (_saveGameManager.CurrentSave != null)
                throw new InvalidOperationException($"A save has already been loaded! ({_saveGameManager.CurrentSave})");

            var saveHeader = MakeSaveGameHeader(name);
            var savePath = ResourcePath.Root / Guid.NewGuid().ToString();
            var save = _saveGameManager.CreateSave(savePath, saveHeader);
            save.LastWriteTime = DateTime.Now;

            return save;
        }

        /// <inheritdoc/>
        public void SaveGame(ISaveGameHandle save)
        {
            BeforeGameSaved?.Invoke(save);

            SaveGlobalData(save);
            SaveSession(save);

            save.Files.Commit();
            save.LastWriteTime = DateTime.Now;
        }

        private void SaveSession(ISaveGameHandle save)
        {
            var activeMap = _mapManager.ActiveMap;

            if (activeMap == null)
            {
                throw new InvalidOperationException("No active map to save");
            }

            DebugTools.Assert(_mapManager.MapIsLoaded(MapId.Global), "No global map!");

            // Save the global map. This is used for global entity storage.
            _mapLoader.SaveMap(MapId.Global, save);

            _mapLoader.SaveMap(activeMap.Id, save);

            var player = _gameSessionManager.Player;
            if (!_entityManager.IsAlive(player))
            {
                throw new InvalidOperationException("No active player");
            }

            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(player);
            if (playerSpatial.MapID != activeMap.Id)
            {
                throw new InvalidOperationException($"Player was not in the active map ({activeMap.Id}) at the time of saving!");
            }

            var sessionData = new SessionData()
            {
                ActiveMapId = (int)activeMap.Id,
                PlayerUid = (int)player,
                NextEntityUid = _entityManager.NextEntityUid,
                NextMapId = _mapManager.NextMapId,
                NextAreaId = _areaManager.NextAreaId,
                Areas = GetSerializableAreas(_areaManager.LoadedAreas)
            };

            var session = new ResourcePath("/session.yml");
            save.Files.WriteSerializedData(session, sessionData, _serializationManager, alwaysWrite: true);
        }

        private Dictionary<AreaId, Area> GetSerializableAreas(Dictionary<AreaId, IArea> loadedAreas)
        {
            return loadedAreas.Values.ToDictionary(area => area.Id, area => (Area)area);
        }

        public void SaveGlobalData(ISaveGameHandle save)
        {
            // Save all the global data not tied to maps.
            var globalData = new MappingDataNode();

            foreach (var (key, reg) in _trackedSaveData)
            {
                Logger.DebugS(SawmillName, $"Saving global data: {key} ({reg.Type})");

                var data = reg.FieldInfo.GetValue(reg.Parent);

                var node = _serializationManager.WriteValue(reg.Type, data, alwaysWrite: true);
                globalData.Add(key, node);
            }

            var root = new MappingDataNode();
            root.Add("data", globalData);

            var global = new ResourcePath("/global.yml");
            save.Files.WriteAllYaml(global, root.ToYaml());
        }

        /// <inheritdoc/>
        public void LoadGame(ISaveGameHandle save)
        {
            save.Files.ClearTemp();

            ResetGameState();

            _saveGameManager.CurrentSave = save;

            LoadSession(save);
            LoadGlobalData(save);

            OnGameLoaded?.Invoke(save);
        }

        private void LoadSession(ISaveGameHandle save)
        {
            var session = new ResourcePath("/session.yml");
            var sessionData = save.Files.ReadSerializedData<SessionData>(session, _serializationManager, skipHook: true)!;

            if (sessionData.NextEntityUid <= (int)EntityUid.Invalid)
                throw new InvalidOperationException($"Invalid NextEntityUid in save!");
            if (sessionData.NextMapId <= (int)MapId.Nullspace)
                throw new InvalidOperationException($"Invalid NextMapId in save!");
            if (sessionData.PlayerUid <= (int)EntityUid.Invalid)
                throw new InvalidOperationException($"Invalid player UID in save!");
            if (sessionData.ActiveMapId <= (int)MapId.Nullspace)
                throw new InvalidOperationException($"Invalid active map ID in save!");

            // Set the next entity UID first since the map loader depends on it.
            _entityManager.NextEntityUid = sessionData.NextEntityUid;

            _mapManager.NextMapId = sessionData.NextMapId;
            var map = _mapLoader.LoadMap(new MapId(sessionData.ActiveMapId), save);

            // Load the global map.
            _mapLoader.LoadMap(MapId.Global, save);

            // Load areas.
            _areaManager.NextAreaId = sessionData.NextAreaId;
            foreach (var (areaId, area) in sessionData.Areas)
            {
                _areaManager.RegisterArea(area, areaId, area.AreaEntityUid);
            }
        
            var playerUid = new EntityUid(sessionData.PlayerUid);
            if (!_entityManager.TryGetComponent(playerUid, out SpatialComponent player) || player.MapID != map.Id)
            {
                throw new InvalidDataException($"Active player '{sessionData.PlayerUid}' was not in saved active map {map.Id}!");
            }

            _gameSessionManager.Player = playerUid;

            // Set the active map. This also lets things like map tile layers that depend
            // on the player being valid refresh themselves correctly.
            _mapManager.SetActiveMap(map.Id, MapLoadType.GameLoaded);
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

                var value = _serializationManager.Read(reg.Type, rawNode, skipHook: true);

                reg.SetValueOnParent(value);
            }
        }

        public void ResetGameState()
        {
            _entityManager.FlushEntities(EntityDeleteType.Unload);
            _mapManager.FlushMaps();
            _areaManager.FlushAreas();
            ResetGlobalSaveData();
            _saveGameManager.CurrentSave = null;
        }
    }
}

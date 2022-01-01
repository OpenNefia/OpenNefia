using JetBrains.Annotations;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    /// Class for loading and saving map blueprints, which are human-readable
    /// YAML files containing map and entity data.
    /// </summary>
    /// <seealso cref="SerialMapLoader"/>
    public class MapBlueprintLoader : IMapBlueprintLoader
    {
        public const string SawmillName = "map.load";

        private const int MapBlueprintFormatVersion = 1;

        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        public event BlueprintEntityLoadedDelegate? OnBlueprintEntityLoaded;

        /// <inheritdoc />
        public void SaveBlueprint(MapId? mapId, ResourcePath resPath)
        {
            if (mapId == null)
                mapId = _mapManager.GetFreeMapId();

            using var profiler = new ProfilerLogger(LogLevel.Debug, SawmillName, $"Map blueprint save: {resPath}");
            Logger.InfoS(SawmillName, $"Saving map {mapId} to {resPath}...");

            var context = new MapBlueprintContext(mapId.Value, _mapManager, _tileDefinitionManager, _entityManager, _prototypeManager);
            var root = context.Serialize();
            var document = new YamlDocument(root);

            _resourceManager.UserData.CreateDirectory(resPath.Directory);

            using (var writer = _resourceManager.UserData.OpenWriteText(resPath))
            {
                var stream = new YamlStream();
                stream.Add(document);
                stream.Save(new YamlMappingFix(new Emitter(writer)), false);
            }
        }

        public IMap LoadBlueprint(MapId? mapId, ResourcePath yamlPath)
        {
            TextReader reader;

            // try user
            if (!_resourceManager.UserData.Exists(yamlPath))
            {
                Logger.DebugS(SawmillName, $"No user blueprint path: {yamlPath}");

                // fallback to content
                if (_resourceManager.TryContentFileRead(yamlPath, out var contentReader))
                {
                    reader = new StreamReader(contentReader);
                }
                else
                {
                    throw new ArgumentException($"No blueprint found: {yamlPath}", nameof(yamlPath));
                }
            }
            else
            {
                reader = _resourceManager.UserData.OpenText(yamlPath);
            }

            using (reader)
            {
                using var profiler = new ProfilerLogger(LogLevel.Debug, SawmillName, $"Map blueprint load: {yamlPath}");
                Logger.InfoS(SawmillName, $"Loading map: {yamlPath}");
                return LoadBlueprint(mapId, reader);
            }
        }

        public IMap LoadBlueprint(MapId? mapId, TextReader reader)
        {
            if (mapId == null)
                mapId = _mapManager.GetFreeMapId();

            var data = new MapData(reader);

            var context = new MapBlueprintContext(mapId.Value, _mapManager, _tileDefinitionManager, _entityManager,
                _prototypeManager, (YamlMappingNode)data.RootNode, OnBlueprintEntityLoaded);
            context.Deserialize();
            var grid = context.MapGrid!;

            foreach (var entityUid in context.Entities)
            {
                entityUid.RunMapInit();
            }

            return grid;
        }

        public static class Keys
        {
            public const string Meta = "meta";
            public const string Meta_Format = "format";
            public const string Meta_Name = "name";
            public const string Meta_Author = "author";

            public const string Tilemap = "tilemap";

            public const string Grid = "grid";

            public const string Entities = "entities";
            public const string Entities_ProtoId = "protoId";
            public const string Entities_Uid = "uid";

            public const string Entities_Components = "components";
            public const string Entities_Components_Type = "type";
        }

        private class MapBlueprintContext : ISerializationContext, IEntityLoadContext,
            ITypeSerializer<EntityUid, ValueDataNode>,
            ITypeReaderWriter<EntityUid, ValueDataNode>
        {

            private MapId _targetMapId;
            private IMapManager _mapManager;
            private ITileDefinitionManager _tileDefinitionManager;
            private IEntityManagerInternal _entityManager;
            private IPrototypeManager _prototypeManager;

            private readonly BlueprintEntityLoadedDelegate? _onBlueprintEntityLoaded;

            public IMap? MapGrid;
            private MapMetadata _mapMetadata = new();
            private readonly Dictionary<EntityUid, int> _entityUidMap = new();
            private readonly Dictionary<int, EntityUid> _uidEntityMap = new();
            public readonly List<EntityUid> Entities = new();

            private readonly YamlMappingNode _rootNode;

            private Dictionary<string, MappingDataNode>? _currentReadingEntityComponents;

            private string? _currentWritingComponent;
            private EntityUid? _currentWritingEntity;

            // { "#" -> "Elona.Wall" }
            private Dictionary<string, PrototypeId<TilePrototype>>? _tileMap;
            // { "Elona.Wall" -> "#" }
            private Dictionary<PrototypeId<TilePrototype>, string>? _tileMapInverse;

            public Dictionary<(Type, Type), object> TypeReaders { get; }
            public Dictionary<Type, object> TypeWriters { get; }
            public Dictionary<Type, object> TypeCopiers => TypeWriters;
            public Dictionary<(Type, Type), object> TypeValidators => TypeReaders;
            public Dictionary<Type, object> TypeComparers => TypeWriters;

            private readonly List<(EntityUid, YamlMappingNode)> _entitiesToDeserialize
                = new();

            /// <summary>
            /// For serialization use.
            /// </summary>
            public MapBlueprintContext(MapId targetMapId,
                IMapManager mapManager,
                ITileDefinitionManager tileDefinitionManager,
                IEntityManagerInternal serverEntityManager,
                IPrototypeManager prototypeManager)
            {
                _targetMapId = targetMapId;
                _mapManager = mapManager;
                _tileDefinitionManager = tileDefinitionManager;
                _entityManager = serverEntityManager;
                _prototypeManager = prototypeManager;

                _rootNode = new YamlMappingNode();
                TypeWriters = new Dictionary<Type, object>()
                {
                    {typeof(EntityUid), this}
                };
                TypeReaders = new Dictionary<(Type, Type), object>()
                {
                    {(typeof(EntityUid), typeof(ValueDataNode)), this}
                };
            }

            /// <summary>
            /// For deserialization use.
            /// </summary>
            public MapBlueprintContext(MapId targetMapId,
                IMapManager mapManager,
                ITileDefinitionManager tileDefinitionManager,
                IEntityManagerInternal serverEntityManager,
                IPrototypeManager prototypeManager,
                YamlMappingNode node,
                BlueprintEntityLoadedDelegate? onLoaded)
            {
                _targetMapId = targetMapId;
                _mapManager = mapManager;
                _tileDefinitionManager = tileDefinitionManager;
                _entityManager = serverEntityManager;
                _prototypeManager = prototypeManager;
                _onBlueprintEntityLoaded = onLoaded;

                _rootNode = node;
                _prototypeManager = prototypeManager;
                TypeWriters = new Dictionary<Type, object>()
                {
                    {typeof(EntityUid), this}
                };
                TypeReaders = new Dictionary<(Type, Type), object>()
                {
                    {(typeof(EntityUid), typeof(ValueDataNode)), this}
                };
            }

            #region Deserialization

            public void Deserialize()
            {
                // Verify that prototypes for all the entities exist and throw if they don't.
                VerifyEntitiesExist();

                // First we load map meta data like version.
                ReadMetaSection();

                // Load grids.
                ReadTileMapSection();
                ReadGridSection();

                // Entities are first allocated. This allows us to know the future UID of all entities on the map before
                // even ExposeData is loaded. This allows us to resolve serialized EntityUid instances correctly.
                AllocEntities();

                // Actually instance components and run ExposeData on them.
                FinishEntitiesLoad();

                FixMapEntity();

                // Run Initialize on all components.
                FinishEntitiesInitialization();

                // Run Startup on all components.
                FinishEntitiesStartup();
            }

            private void VerifyEntitiesExist()
            {
                var fail = false;
                var entities = _rootNode.GetNode<YamlSequenceNode>(Keys.Entities);
                var reportedError = new HashSet<PrototypeId<EntityPrototype>>();
                foreach (var entityDef in entities.Cast<YamlMappingNode>())
                {
                    if (entityDef.TryGetNode(Keys.Entities_ProtoId, out var typeNode))
                    {
                        var protoId = new PrototypeId<EntityPrototype>(typeNode.AsString());
                        if (!_prototypeManager.HasIndex(protoId) && !reportedError.Contains(protoId))
                        {
                            Logger.ErrorS(MapBlueprintLoader.SawmillName, "Missing prototype for map: {0}", protoId);
                            fail = true;
                            reportedError.Add(protoId);
                        }
                    }
                }

                if (fail)
                {
                    throw new InvalidOperationException(
                        "Found missing prototypes in map file. Missing prototypes have been dumped to logs.");
                }
            }

            private void ReadMetaSection()
            {
                var meta = _rootNode.GetNode<YamlMappingNode>(Keys.Meta);
                var ver = meta.GetNode(Keys.Meta_Format).AsInt();
                if (ver != MapBlueprintFormatVersion)
                {
                    throw new InvalidDataException("Cannot handle this map blueprint file version.");
                }

                var name = meta.GetNode(Keys.Meta_Name).AsString();
                var author = meta.GetNode(Keys.Meta_Author).AsString();
                _mapMetadata = new MapMetadata(name, author);
            }

            private void ReadTileMapSection()
            {
                // Load tile mapping so that we can map the stored tile IDs into the ones actually used at runtime.
                _tileMap = new Dictionary<string, PrototypeId<TilePrototype>>();

                var tileMap = _rootNode.GetNode<YamlMappingNode>(Keys.Tilemap);
                foreach (var (key, value) in tileMap)
                {
                    var tileRune = key.AsString(); // Something like '#' or '.' or '$'.
                    if (tileRune.GetWideLength() != 1)
                    {
                        throw new InvalidDataException($"Tilemap runes must be a single character wide, got: {tileRune}");
                    }
                    var tileProtoId = new PrototypeId<TilePrototype>(value.AsString());
                    _tileMap.Add(tileRune, tileProtoId);
                }
            }

            private void ReadGridSection()
            {
                var gridString = _rootNode.GetNode(Keys.Grid).AsString().Trim();

                MapGrid = YamlGridSerializer.DeserializeGrid(gridString, _tileMap!);
            }

            private void AllocEntities()
            {
                var entities = _rootNode.GetNode<YamlSequenceNode>(Keys.Entities);
                foreach (var entityDef in entities.Cast<YamlMappingNode>())
                {
                    PrototypeId<EntityPrototype>? protoId = null;
                    if (entityDef.TryGetNode(Keys.Entities_ProtoId, out var typeNode))
                    {
                        protoId = new PrototypeId<EntityPrototype>(typeNode.AsString());
                    }

                    var uid = Entities.Count;
                    if (entityDef.TryGetNode(Keys.Entities_Uid, out var uidNode))
                    {
                        uid = uidNode.AsInt();
                    }

                    var entity = _entityManager.AllocEntity(protoId);
                    Entities.Add(entity.Uid);
                    _uidEntityMap.Add(uid, entity.Uid);
                    _entitiesToDeserialize.Add((entity.Uid, entityDef));

                    var comp = _entityManager.AddComponent<MapSaveIdComponent>(entity);
                    comp.Uid = uid;
                }
            }

            private void FinishEntitiesLoad()
            {
                foreach (var (entity, data) in _entitiesToDeserialize)
                {
                    _currentReadingEntityComponents = new Dictionary<string, MappingDataNode>();
                    if (data.TryGetNode(Keys.Entities_Components, out YamlSequenceNode? componentList))
                    {
                        foreach (var compData in componentList.Cast<YamlMappingNode>())
                        {
                            var mapping = compData.ToDataNodeCast<MappingDataNode>();
                            var copy = mapping.Copy();
                            copy.Remove(Keys.Entities_Components_Type);
                            _currentReadingEntityComponents[compData[Keys.Entities_Components_Type].AsString()] = copy;
                        }
                    }

                    _entityManager.FinishEntityLoad(_entityManager.GetEntity(entity), this);

                    _onBlueprintEntityLoaded?.Invoke(entity);
                }
            }

            private EntityUid FindMapEntity()
            {
                EntityUid found = EntityUid.Invalid;

                foreach (var entity in Entities)
                {
                    if (_entityManager.HasComponent<MapComponent>(entity))
                    {
                        if (found.IsValid())
                        {
                            throw new InvalidDataException($"Map blueprint has more than one entity with a {nameof(MapComponent)}");
                        }

                        found = entity;
                    }
                }

                if (!found.IsValid())
                {
                    throw new InvalidDataException($"Map blueprint does not contain an entity with a {nameof(MapComponent)}");
                }

                return found;
            }

            private void FixMapEntity()
            {
                var mapEntityInBlueprint = FindMapEntity();
                var mapComponent = _entityManager.EnsureComponent<MapComponent>(mapEntityInBlueprint);
                mapComponent.MapId = _targetMapId;
                mapComponent.Metadata = _mapMetadata;
                _targetMapId = _mapManager.RegisterMap(MapGrid!, _targetMapId, mapEntityInBlueprint);
            }

            private void FinishEntitiesInitialization()
            {
                foreach (var entity in Entities)
                {
                    _entityManager.FinishEntityInitialization(_entityManager.GetEntity(entity));
                }
            }

            private void FinishEntitiesStartup()
            {
                foreach (var entity in Entities)
                {
                    _entityManager.FinishEntityStartup(_entityManager.GetEntity(entity));
                }
            }

            #endregion

            #region Serialization

            public YamlNode Serialize()
            {
                MapGrid = _mapManager.GetMap(_targetMapId);

                WriteMetaSection();
                WriteTileMapSection();
                WriteGridSection();

                PopulateEntityList();
                WriteEntitySection();

                return _rootNode;
            }

            private void WriteMetaSection()
            {
                var mapEntity = _mapManager.GetMapEntity(_targetMapId);
                var mapComp = _entityManager.EnsureComponent<MapComponent>(mapEntity.Uid);

                var meta = new YamlMappingNode();
                _rootNode.Add(Keys.Meta, meta);
                meta.Add(Keys.Meta_Format, MapBlueprintFormatVersion.ToString(CultureInfo.InvariantCulture));
                meta.Add(Keys.Meta_Name, mapComp.Metadata.Name);
                meta.Add(Keys.Meta_Author, mapComp.Metadata.Author);
            }

            private void WriteTileMapSection()
            {
                _tileMapInverse = YamlGridSerializer.BuildProtoToRuneTileMap(MapGrid!);

                var tileMap = new YamlMappingNode();
                _rootNode.Add(Keys.Tilemap, tileMap);

                foreach (var pair in _tileMapInverse)
                {
                    tileMap.Add(pair.Value, pair.Key.ToString());
                }
            }

            private void WriteGridSection()
            {
                var grid = new YamlScalarNode(YamlGridSerializer.SerializeGrid(MapGrid!, _tileMapInverse!, _tileDefinitionManager));
                grid.Style = ScalarStyle.Literal;
                _rootNode.Add(Keys.Grid, grid);
            }

            private void PopulateEntityList()
            {
                var withUid = new List<MapSaveIdComponent>();
                var withoutUid = new List<EntityUid>();
                var takenIds = new HashSet<int>();

                foreach (var entityUid in _entityManager.GetEntityUids())
                {
                    if (IsMapSavable(entityUid))
                    {
                        Entities.Add(entityUid);
                        if (_entityManager.TryGetComponent(entityUid, out MapSaveIdComponent? mapSaveId))
                        {
                            withUid.Add(mapSaveId);
                        }
                        else
                        {
                            withoutUid.Add(entityUid);
                        }
                    }
                }

                // Go over entities with a MapSaveIdComponent and assign those.

                foreach (var mapIdComp in withUid)
                {
                    var uid = mapIdComp.Uid;
                    if (takenIds.Contains(uid))
                    {
                        // Duplicate ID. Just pretend it doesn't have an ID and use the without path.
                        withoutUid.Add(mapIdComp.OwnerUid);
                    }
                    else
                    {
                        _entityUidMap.Add(mapIdComp.OwnerUid, uid);
                        takenIds.Add(uid);
                    }
                }

                var uidCounter = 0;
                foreach (var entity in withoutUid)
                {
                    while (takenIds.Contains(uidCounter))
                    {
                        // Find next available UID.
                        uidCounter += 1;
                    }

                    _entityUidMap.Add(entity, uidCounter);
                    takenIds.Add(uidCounter);
                }
            }

            private void WriteEntitySection()
            {
                var serializationManager = IoCManager.Resolve<ISerializationManager>();
                var entities = new YamlSequenceNode();
                _rootNode.Add(Keys.Entities, entities);

                var prototypeCompCache = new Dictionary<string, Dictionary<string, MappingDataNode>>();
                foreach (var entity in Entities.OrderBy(e => _entityUidMap[e]))
                {
                    _currentWritingEntity = entity;
                    var mapping = new YamlMappingNode
                    {
                        {Keys.Entities_Uid, _entityUidMap[entity].ToString(CultureInfo.InvariantCulture)}
                    };

                    if (_entityManager.GetComponent<MetaDataComponent>(entity).EntityPrototype is { } prototype)
                    {
                        mapping.Add(Keys.Entities_ProtoId, prototype.ID);
                        if (!prototypeCompCache.ContainsKey(prototype.ID))
                        {
                            prototypeCompCache[prototype.ID] = new Dictionary<string, MappingDataNode>();
                            foreach (var (compType, comp) in prototype.Components)
                            {
                                prototypeCompCache[prototype.ID].Add(compType, serializationManager.WriteValueAs<MappingDataNode>(comp.GetType(), comp));
                            }
                        }
                    }

                    var components = new YamlSequenceNode();
                    foreach (var component in _entityManager.GetComponents(entity))
                    {
                        if (component is MapSaveIdComponent)
                            continue;

                        _currentWritingComponent = component.Name;
                        var compMapping = serializationManager.WriteValueAs<MappingDataNode>(component.GetType(), component, context: this);

                        var md = _entityManager.GetComponent<MetaDataComponent>(entity);
                        if (md.EntityPrototype != null && prototypeCompCache[md.EntityPrototype.ID].TryGetValue(component.Name, out var protMapping))
                        {
                            compMapping = compMapping.Except(protMapping);
                            if (compMapping == null) continue;
                        }

                        // Don't need to write it if nothing was written!
                        if (compMapping.Children.Count != 0)
                        {
                            compMapping.Add(Keys.Entities_Components_Type, new ValueDataNode(component.Name));
                            // Something actually got written!
                            components.Add(compMapping.ToYamlNode());
                        }
                    }

                    if (components.Children.Count != 0)
                    {
                        mapping.Add(Keys.Entities_Components, components);
                    }

                    entities.Add(mapping);
                }
            }

            // Create custom object serializers that will correctly allow data to be overriden by the map file.
            IComponent IEntityLoadContext.GetComponentData(string componentName,
                IComponent? protoData)
            {
                if (_currentReadingEntityComponents == null)
                {
                    throw new InvalidOperationException();
                }

                var serializationManager = IoCManager.Resolve<ISerializationManager>();
                var factory = IoCManager.Resolve<IComponentFactory>();

                IComponent data = protoData != null
                    ? serializationManager.CreateCopy(protoData, this)!
                    : (IComponent)Activator.CreateInstance(factory.GetRegistration(componentName).Type)!;

                if (_currentReadingEntityComponents.TryGetValue(componentName, out var mapping))
                {
                    var mapData = (IDeserializedDefinition)serializationManager.Read(
                        factory.GetRegistration(componentName).Type,
                        mapping, this);
                    var newData = serializationManager.PopulateDataDefinition(data, mapData);
                    data = (IComponent)newData.RawValue!;
                }

                return data;
            }

            public IEnumerable<string> GetExtraComponentTypes()
            {
                return _currentReadingEntityComponents!.Keys;
            }

            private bool IsMapSavable(EntityUid entity)
            {
                if (_entityManager.GetComponent<MetaDataComponent>(entity).EntityPrototype?.MapSavable == false)
                {
                    return false;
                }

                // Don't serialize things parented to unsavable things.
                var current = _entityManager.GetComponent<SpatialComponent>(entity);
                foreach (var parent in current.Parents)
                {
                    if (_entityManager.GetComponent<MetaDataComponent>(parent.OwnerUid).EntityPrototype?.MapSavable == false)
                    {
                        return false;
                    }
                }

                return true;
            }

            ValidationNode ITypeValidator<EntityUid, ValueDataNode>.Validate(ISerializationManager serializationManager,
                ValueDataNode node, IDependencyCollection dependencies, ISerializationContext? context)
            {
                if (node.Value == "null")
                {
                    return new ValidatedValueNode(node);
                }

                if (!int.TryParse(node.Value, out var val) || !_uidEntityMap.ContainsKey(val))
                {
                    return new ErrorNode(node, $"Invalid {nameof(EntityUid)}", true);
                }

                return new ValidatedValueNode(node);
            }

            public DataNode Write(ISerializationManager serializationManager, EntityUid value, bool alwaysWrite = false,
                ISerializationContext? context = null)
            {
                if (!_entityUidMap.TryGetValue(value, out var _entityUidMapped))
                {
                    Logger.WarningS(MapBlueprintLoader.SawmillName, "Cannot write entity UID '{0}'.", value);
                    return new ValueDataNode("null");
                }
                else
                {
                    return new ValueDataNode(_entityUidMapped.ToString(CultureInfo.InvariantCulture));
                }
            }

            DeserializationResult ITypeReader<EntityUid, ValueDataNode>.Read(ISerializationManager serializationManager,
                ValueDataNode node,
                IDependencyCollection dependencies,
                bool skipHook,
                ISerializationContext? context)
            {
                if (node.Value == "null")
                {
                    return new DeserializedValue<EntityUid>(EntityUid.Invalid);
                }

                var val = int.Parse(node.Value);

                if (val >= Entities.Count || !_uidEntityMap.ContainsKey(val) || !Entities.TryFirstOrNull(e => e == _uidEntityMap[val], out var entity))
                {
                    Logger.ErrorS(MapBlueprintLoader.SawmillName, "Error in map file: found local entity UID '{0}' which does not exist.", val);
                    return null!;
                }
                else
                {
                    return new DeserializedValue<EntityUid>(entity!.Value);
                }
            }

            [MustUseReturnValue]
            public EntityUid Copy(ISerializationManager serializationManager, EntityUid source, EntityUid target,
                bool skipHook,
                ISerializationContext? context = null)
            {
                return new((int)source);
            }

            public bool Compare(ISerializationManager serializationManager, EntityUid left, EntityUid right,
                bool skipHook, ISerializationContext? context = null)
            {
                return left == right;
            }

            #endregion
        }

        /// <summary>
        ///     Does basic pre-deserialization checks on map file load.
        /// </summary>
        private class MapData
        {
            public YamlStream Stream { get; }

            public YamlNode RootNode => Stream.Documents[0].RootNode;

            public MapData(TextReader reader)
            {
                var stream = new YamlStream();
                stream.Load(reader);

                if (stream.Documents.Count < 1)
                {
                    throw new InvalidDataException("Stream has no YAML documents.");
                }

                // Kinda wanted to just make this print a warning and pick [0] but screw that.
                // What is this, a hug box?
                if (stream.Documents.Count > 1)
                {
                    throw new InvalidDataException("Stream too many YAML documents. Map blueprints store exactly one.");
                }

                Stream = stream;
            }
        }
    }
}

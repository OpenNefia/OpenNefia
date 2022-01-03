using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Maps
{
    internal sealed class MapDeserializer
    {
        [Dependency] private readonly IResourceManager _resourceManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        private readonly BlueprintEntityStartupDelegate? _onBlueprintEntityStartup;

        private MapSerializeMode _mode;
        private MapId _targetMapId;
        private readonly YamlMappingNode _rootNode;
        private MapSerializationContext _context;

        private MapMetadata _mapMetadata = new();

        // { "#" -> "Elona.Wall" }
        private Dictionary<string, PrototypeId<TilePrototype>> _tileMap = new();
        
        public IEnumerable<EntityUid> Entities => _context.Entities;

        public IMap? MapGrid { get; private set; }

        private readonly List<(EntityUid, YamlMappingNode)> _entitiesToDeserialize = new();

        public MapDeserializer(MapId targetMapId, MapSerializeMode mode,
            YamlMappingNode node,
            BlueprintEntityStartupDelegate? onLoaded)
        {
            IoCManager.InjectDependencies(this);

            _context = new MapSerializationContext(mode, _serializationManager);

            _targetMapId = targetMapId;
            _mode = mode;
            _rootNode = node;
            _onBlueprintEntityStartup = onLoaded;
        }

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
            var entities = _rootNode.GetNode<YamlSequenceNode>(MapLoadConstants.Entities);
            var reportedError = new HashSet<PrototypeId<EntityPrototype>>();
            foreach (var entityDef in entities.Cast<YamlMappingNode>())
            {
                if (entityDef.TryGetNode(MapLoadConstants.Entities_ProtoId, out var typeNode))
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
            var meta = _rootNode.GetNode<YamlMappingNode>(MapLoadConstants.Meta);
            var ver = meta.GetNode(MapLoadConstants.Meta_Format).AsInt();
            if (ver != MapLoadConstants.MapBlueprintFormatVersion)
            {
                throw new InvalidDataException("Cannot handle this map blueprint file version.");
            }

            var name = meta.GetNode(MapLoadConstants.Meta_Name).AsString();
            var author = meta.GetNode(MapLoadConstants.Meta_Author).AsString();
            _mapMetadata = new MapMetadata(name, author);
        }

        private void ReadTileMapSection()
        {
            // Load tile mapping so that we can map the stored tile IDs into the ones actually used at runtime.
            _tileMap = new Dictionary<string, PrototypeId<TilePrototype>>();

            var tileMap = _rootNode.GetNode<YamlMappingNode>(MapLoadConstants.Tilemap);
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
            var gridString = _rootNode.GetNode(MapLoadConstants.Grid).AsString().Trim();

            MapGrid = YamlGridSerializer.DeserializeGrid(gridString, _tileMap!);
        }

        private void AllocEntities()
        {
            var entities = _rootNode.GetNode<YamlSequenceNode>(MapLoadConstants.Entities);
            foreach (var entityDef in entities.Cast<YamlMappingNode>())
            {
                PrototypeId<EntityPrototype>? protoId = null;
                if (entityDef.TryGetNode(MapLoadConstants.Entities_ProtoId, out var typeNode))
                {
                    protoId = new PrototypeId<EntityPrototype>(typeNode.AsString());
                }

                int uid;
                EntityUid? realUid;

                switch (_mode)
                {
                    case MapSerializeMode.Blueprint:
                        uid = _context.Entities.Count;
                        if (entityDef.TryGetNode(MapLoadConstants.Entities_Uid, out var uidNode))
                        {
                            uid = uidNode.AsInt();
                        }
                        realUid = null;
                        break;
                    case MapSerializeMode.Full:
                    default:
                        uid = entityDef.GetNode(MapLoadConstants.Entities_Uid).AsInt();
                        realUid = new EntityUid(uid);
                        break;
                }

                var entity = _entityManager.AllocEntity(protoId, realUid);
                _context.Entities.Add(entity.Uid);
                _context.UidEntityMap.Add(uid, entity.Uid);
                _entitiesToDeserialize.Add((entity.Uid, entityDef));

                if (_mode == MapSerializeMode.Blueprint)
                {
                    var comp = _entityManager.AddComponent<MapSaveIdComponent>(entity);
                    comp.Uid = uid;
                }
            }
        }

        private void FinishEntitiesLoad()
        {
            foreach (var (entity, data) in _entitiesToDeserialize)
            {
                _context.CurrentReadingEntityComponents = new Dictionary<string, MappingDataNode>();
                if (data.TryGetNode(MapLoadConstants.Entities_Components, out YamlSequenceNode? componentList))
                {
                    foreach (var compData in componentList.Cast<YamlMappingNode>())
                    {
                        var mapping = compData.ToDataNodeCast<MappingDataNode>();
                        var copy = mapping.Copy();
                        copy.Remove(MapLoadConstants.Entities_Components_Type);
                        _context.CurrentReadingEntityComponents[compData[MapLoadConstants.Entities_Components_Type].AsString()] = copy;
                    }

                    if (_mode == MapSerializeMode.Full)
                    {
                        // Remove extra components found in the prototype that were not present on the
                        // entity at the time of saving.
                        foreach (var comp in _entityManager.GetComponents(entity).ToList())
                        {
                            if (!_context.CurrentReadingEntityComponents.ContainsKey(comp.Name))
                            {
                                _entityManager.RemoveComponent(entity, comp);
                            }
                        }
                    }
                }

                _entityManager.FinishEntityLoad(_entityManager.GetEntity(entity), _context);
            }
        }

        private EntityUid FindMapEntity()
        {
            EntityUid found = EntityUid.Invalid;

            foreach (var entity in _context.Entities)
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
            foreach (var entity in _context.Entities)
            {
                _entityManager.FinishEntityInitialization(_entityManager.GetEntity(entity));
            }
        }

        private void FinishEntitiesStartup()
        {
            foreach (var entity in _context.Entities)
            {
                _entityManager.FinishEntityStartup(_entityManager.GetEntity(entity));

                _onBlueprintEntityStartup?.Invoke(entity);
            }
        }
    }
}
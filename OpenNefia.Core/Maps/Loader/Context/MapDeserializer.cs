using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace OpenNefia.Core.Maps
{
    internal sealed class MapDeserializer
    {
        [Dependency] private readonly IMapManagerInternal _mapManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private readonly BlueprintEntityStartupDelegate? _onBlueprintEntityStartup;

        private MapSerializeMode _mode;
        private MapId _targetMapId;
        private readonly MappingDataNode _rootNode;
        private MapSerializationContext _context;

        private MapMetadata _mapMetadata = new();

        // { "#" -> "Elona.Wall" }
        private Dictionary<string, PrototypeId<TilePrototype>> _tileMap = new();
        
        public IEnumerable<EntityUid> Entities => _context.Entities;

        public Map? MapGrid { get; private set; }

        private readonly List<(EntityUid, MappingDataNode)> _entitiesToDeserialize = new();

        public MapDeserializer(MapId targetMapId, MapSerializeMode mode,
            MappingDataNode node,
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
            if (_mode == MapSerializeMode.Full)
            {
                ReadGridMemorySection();
                ReadGridInSightSections();
                ReadObjectMemorySection();
            }

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

            // Recalculate solidity/opacity for all tiles, taking entity spatials in to account.
            RecalculateTileTangibility();

            // Run MapInit on all entities. Can only do this after tile solidity has been calculated.
            RunMapInitEventsOnMapAndEntities();
        }

        private void VerifyEntitiesExist()
        {
            var fail = false;
            var entities = _rootNode.Get<SequenceDataNode>(MapLoadConstants.Entities);
            var reportedError = new HashSet<PrototypeId<EntityPrototype>>();
            foreach (var entityDef in entities.Cast<MappingDataNode>())
            {
                if (entityDef.TryGet<ValueDataNode>(MapLoadConstants.Entities_ProtoId, out var typeNode))
                {
                    var protoId = new PrototypeId<EntityPrototype>(typeNode.Value);
                    if (!_prototypeManager.HasIndex(protoId) && !reportedError.Contains(protoId))
                    {
                        Logger.ErrorS(MapLoader.SawmillName, "Missing prototype for map: {0}", protoId);
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
            var meta = _rootNode.Get<MappingDataNode>(MapLoadConstants.Meta);
            var ver = meta.Get<ValueDataNode>(MapLoadConstants.Meta_Format).AsInt();
            if (ver != MapLoadConstants.MapBlueprintFormatVersion)
            {
                throw new InvalidDataException("Cannot handle this map blueprint file version.");
            }

            var name = meta.Get<ValueDataNode>(MapLoadConstants.Meta_Name).Value;
            var author = meta.Get<ValueDataNode>(MapLoadConstants.Meta_Author).Value;
            _mapMetadata = new MapMetadata(name, author);
        }

        private void ReadTileMapSection()
        {
            // Load tile mapping so that we can map the stored tile IDs into the ones actually used at runtime.
            _tileMap = new Dictionary<string, PrototypeId<TilePrototype>>();

            var tileMap = _rootNode.Get<MappingDataNode>(MapLoadConstants.Tilemap);
            foreach (var (key, value) in tileMap)
            {
                var keyNode = (ValueDataNode)key;
                var valueNode = (ValueDataNode)value;

                var tileRune = keyNode.Value; // Something like '#' or '.' or '$'.
                if (tileRune.GetWideLength() != 1)
                {
                    throw new InvalidDataException($"Tilemap runes must be a single character wide, got: {tileRune}");
                }
                var tileProtoId = new PrototypeId<TilePrototype>(valueNode.Value);
                _tileMap.Add(tileRune, tileProtoId);
            }
        }

        private void ReadGridSection()
        {
            var gridString = _rootNode.Get<ValueDataNode>(MapLoadConstants.Grid).Value.Trim();

            var tiles = YamlGridSerializer.DeserializeGrid(gridString, _tileMap!, _tileDefinitionManager, out var tileMapSize);

            MapGrid = new Map(tileMapSize.X, tileMapSize.Y);
            foreach (var tile in MapGrid.AllTiles)
            {
                MapGrid.Tiles[tile.X, tile.Y] = tiles[tile.X, tile.Y];
            }

            MapGrid.RedrawAllThisTurn = true;
        }

        private void ReadGridMemorySection()
        {
            var gridMemoryString = _rootNode.Get<ValueDataNode>(MapLoadConstants.GridMemory).Value.Trim();
            var tileMemory = YamlGridSerializer.DeserializeGrid(gridMemoryString, _tileMap!, _tileDefinitionManager, out var tileMapMemorySize);

            if (tileMapMemorySize != MapGrid!.Size)
            {
                throw new InvalidDataException($"Tilemap memory size {tileMapMemorySize} was not the same as tilemap size {MapGrid.Size}!");
            }

            foreach (var tile in MapGrid.AllTileMemory)
            {
                MapGrid.TileMemory[tile.X, tile.Y] = tileMemory[tile.X, tile.Y];
            }
        }

        private void ReadGridInSightSections()
        {
            var gridInSightStr = _rootNode.Get<ValueDataNode>(MapLoadConstants.GridInSight).Value;
            MapGrid!.InSight = YamlGridSerializer.DeserializeInSight(gridInSightStr, MapGrid.Size);
            MapGrid!.LastSightId = uint.Parse(_rootNode.Get<ValueDataNode>(MapLoadConstants.GridLastSightId).Value);
        }

        private void ReadObjectMemorySection()
        {
            var objectMemoryNode = _rootNode.Get<MappingDataNode>(MapLoadConstants.ObjectMemory);
            
            if (objectMemoryNode == null)
            {
                throw new InvalidDataException($"Object memory section '{MapLoadConstants.ObjectMemory}' not found!");
            }

            MapGrid!.MapObjectMemory = _serializationManager.Read<MapObjectMemoryStore>(objectMemoryNode);

            foreach (var memory in MapGrid.MapObjectMemory.AllMemory.Values)
            {
                foreach (var drawable in memory.Drawables)
                {
                    EntitySystem.InjectDependencies(drawable);
                    drawable.Initialize(_resourceCache);
                }
            }
        }

        private void AllocEntities()
        {
            var entities = _rootNode.Get<SequenceDataNode>(MapLoadConstants.Entities);
            foreach (var entityDef in entities.Cast<MappingDataNode>())
            {
                PrototypeId<EntityPrototype>? protoId = null;
                if (entityDef.TryGet<ValueDataNode>(MapLoadConstants.Entities_ProtoId, out var typeNode))
                {
                    protoId = new PrototypeId<EntityPrototype>(typeNode.Value);
                }

                int uid;
                EntityUid? realUid;

                switch (_mode)
                {
                    case MapSerializeMode.Blueprint:
                        uid = _context.Entities.Count;
                        if (entityDef.TryGet<ValueDataNode>(MapLoadConstants.Entities_Uid, out var uidNode))
                        {
                            uid = uidNode.AsInt();
                        }
                        realUid = null;
                        break;
                    case MapSerializeMode.Full:
                    default:
                        uid = int.Parse(entityDef.Get<ValueDataNode>(MapLoadConstants.Entities_Uid).Value);
                        realUid = new EntityUid(uid);
                        break;
                }

                var entity = _entityManager.AllocEntity(protoId, realUid);
                _context.Entities.Add(entity);
                _context.UidEntityMap.Add(uid, entity);
                _entitiesToDeserialize.Add((entity, entityDef));

                if (_mode == MapSerializeMode.Blueprint)
                {
                    var comp = _entityManager.AddComponent<MapSaveIdComponent>(entity);
                    comp.Uid = uid;
                }
            }
        }

        /// <summary>
        /// { entityPrototypeId -> [compName] }
        /// </summary>
        private class PrototypeCompTypeCache : Dictionary<string, List<string>>
        {
        }

        private void FinishEntitiesLoad()
        {
            var prototypeCompTypeCache = new PrototypeCompTypeCache();

            foreach (var (entity, data) in _entitiesToDeserialize)
            {
                _context.CurrentReadingEntityComponents = new Dictionary<string, MappingDataNode>();
                _context.CurrentDeletedEntityComponents = new HashSet<string>();
                if (data.TryGet<SequenceDataNode>(MapLoadConstants.Entities_Components, out var componentList))
                {
                    foreach (var compData in componentList.Cast<MappingDataNode>())
                    {
                        var copy = compData.Copy();
                        copy.Remove(MapLoadConstants.Entities_Components_Type);
                        var compType = compData.Get<ValueDataNode>(MapLoadConstants.Entities_Components_Type).Value;
                        _context.CurrentReadingEntityComponents[compType] = copy;
                    }
                }

                if (_mode == MapSerializeMode.Full)
                {
                    if (_entityManager.GetComponent<MetaDataComponent>(entity).EntityPrototype is { } prototype)
                    {
                        if (!prototypeCompTypeCache.ContainsKey(prototype.ID))
                        {
                            prototypeCompTypeCache[prototype.ID] = prototype.Components.Keys.ToList();
                        }

                        // Remove extra components found in the prototype that were not present on the
                        // entity at the time of saving.
                        foreach (var compName in prototypeCompTypeCache[prototype.ID])
                        {
                            if (!_context.CurrentReadingEntityComponents.ContainsKey(compName))
                            {
                                _context.CurrentDeletedEntityComponents.Add(compName);
                            }
                        }
                    }
                }

                _entityManager.FinishEntityLoad(entity, _context);
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
            _mapManager.RegisterMap(MapGrid!, _targetMapId, mapEntityInBlueprint);

            var mapLookup = _entityManager.EnsureComponent<MapEntityLookupComponent>(mapEntityInBlueprint);
            mapLookup.InitializeFromMap(MapGrid!);
        }

        private void FinishEntitiesInitialization()
        {
            foreach (var entity in _context.Entities)
            {
                _entityManager.FinishEntityInitialization(entity);
            }
        }

        private void FinishEntitiesStartup()
        {
            foreach (var entity in _context.Entities)
            {
                _entityManager.FinishEntityStartup(entity);

                if (_mode == MapSerializeMode.Blueprint)
                {
                    _onBlueprintEntityStartup?.Invoke(entity);
                }
            }
        }

        private void RunMapInitEventsOnMapAndEntities()
        {
            // This can't go in RegisterMap() because entities haven't been initialized
            // at that point.
            // Maybe run initialize/startup for just the map entity first?

            if (_mode == MapSerializeMode.Blueprint)
            {
                var ev = new MapCreatedEvent(MapGrid!);
                _entityManager.EventBus.RaiseEvent(MapGrid!.MapEntityUid, ev);
                var ev2 = new MapCreatedFromBlueprintEvent(MapGrid!);
                _entityManager.EventBus.RaiseEvent(MapGrid!.MapEntityUid, ev2);
            }
            else if (_mode == MapSerializeMode.Full)
            {
                var ev = new MapLoadedFromSaveEvent(MapGrid!);
                _entityManager.EventBus.RaiseEvent(MapGrid!.MapEntityUid, ev);
            }

            foreach (var entityUid in _context.Entities)
            {
                MapInitExt.RunMapInit(entityUid);
            }
        }

        private void RecalculateTileTangibility()
        {
            var mapLookupComp = _entityManager.GetComponent<MapEntityLookupComponent>(MapGrid!.MapEntityUid);

            foreach (var tile in MapGrid!.AllTiles)
            {
                var pos = tile.Position;
                MapGrid.RefreshTile(pos);

                // this should be the same logic as IEntityLookup.GetLiveEntitiesAtCoords.
                var ents = mapLookupComp.EntitySpatial[pos.X, pos.Y]
                    .Where(uid => _entityManager.IsAlive(uid))
                    .Select(uid => _entityManager.GetComponent<SpatialComponent>(uid));

                MapGrid.RefreshTileEntities(pos, ents);
            }
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class MapCreatedFromBlueprintEvent : EntityEventArgs
    {
        public IMap Map { get; }

        public MapCreatedFromBlueprintEvent(IMap map)
        {
            Map = map;
        }
    }
}
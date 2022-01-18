using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Timing;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Maps
{
    internal enum MapSerializeMode
    {
        /// <summary>
        /// Entities are serialized according to their prototypes. Extra/deleted
        /// components on entities will be ignored.
        /// </summary>
        Blueprint,

        /// <summary>
        /// The exact component list of every entity will be written.
        /// </summary>
        Full
    }

    internal sealed class MapSerializer
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        private readonly MapSerializeMode _mode;
        private readonly YamlMappingNode _rootNode;
        private MapId _targetMapId;
        private readonly MapSerializationContext _context;

        public IMap? MapGrid { get; private set; }

        // { "Elona.Wall" -> "#" }
        private Dictionary<PrototypeId<TilePrototype>, string>? _tileMapInverse;

        public MapSerializer(MapId targetMapId, MapSerializeMode mode)
        {
            IoCManager.InjectDependencies(this);

            _context = new MapSerializationContext(mode, _serializationManager);

            _targetMapId = targetMapId;
            _mode = mode;
            _rootNode = new YamlMappingNode();
        }

        public YamlNode Serialize()
        {
            MapGrid = _mapManager.GetMap(_targetMapId);

            WriteMetaSection();
            WriteTileMapSection();
            WriteGridSection();
            if (_mode == MapSerializeMode.Full)
            {
                WriteGridMemorySection();
                WriteGridInSightSections();
                WriteObjectMemorySection();
            }

            PopulateEntityList();
            WriteEntitySection();

            return _rootNode;
        }

        private void WriteMetaSection()
        {
            var mapEntity = _mapManager.GetMap(_targetMapId).MapEntityUid;
            var mapComp = _entityManager.EnsureComponent<MapComponent>(mapEntity);

            var meta = new YamlMappingNode();
            _rootNode.Add(MapLoadConstants.Meta, meta);
            meta.Add(MapLoadConstants.Meta_Format, MapLoadConstants.MapBlueprintFormatVersion.ToString(CultureInfo.InvariantCulture));
            meta.Add(MapLoadConstants.Meta_Name, mapComp.Metadata.Name);
            meta.Add(MapLoadConstants.Meta_Author, mapComp.Metadata.Author);
        }

        private void WriteTileMapSection()
        {
            _tileMapInverse = YamlGridSerializer.BuildProtoToRuneTileMap(MapGrid!, _tileDefinitionManager);

            var tileMap = new YamlMappingNode();
            _rootNode.Add(MapLoadConstants.Tilemap, tileMap);

            foreach (var pair in _tileMapInverse)
            {
                tileMap.Add(pair.Value, pair.Key.ToString());
            }
        }

        private void WriteGridSection() => DoWriteGrid(MapLoadConstants.Grid, MapGrid!, MapGrid!.Tiles);
        private void WriteGridMemorySection() => DoWriteGrid(MapLoadConstants.GridMemory, MapGrid!, MapGrid!.TileMemory);

        private void DoWriteGrid(string name, IMap map, Tile[,] tiles)
        {
            var grid = new YamlScalarNode(YamlGridSerializer.SerializeGrid(tiles, map.Size, _tileMapInverse!, _tileDefinitionManager));
            grid.Style = ScalarStyle.Literal;
            _rootNode.Add(name, grid);
        }

        private void WriteGridInSightSections()
        {
            var gridInSight = new YamlScalarNode(YamlGridSerializer.SerializeInSight(MapGrid!.InSight, MapGrid.Size));
            _rootNode.Add(MapLoadConstants.GridInSight, gridInSight);
            _rootNode.Add(MapLoadConstants.GridLastSightId, new YamlScalarNode(MapGrid.LastSightId.ToString()));
        }

        private void WriteObjectMemorySection()
        {
            var objectMemory = _serializationManager.WriteValueAs<MappingDataNode>(MapGrid!.MapObjectMemory);
            _rootNode.Add(MapLoadConstants.ObjectMemory, objectMemory.ToYamlNode());
        }

        private IEnumerable<EntityUid> GetAllEntitiesInMap(MapId mapId)
        {
            return _entityManager.GetAllComponents<SpatialComponent>()
                .Where(spatial => spatial.MapID == mapId)
                .Select(spatial => spatial.Owner);
        }

        private void PopulateEntityList()
        {
            switch (_mode)
            {
                case MapSerializeMode.Blueprint:
                    PopulateEntityListBlueprint();
                    break;
                case MapSerializeMode.Full:
                default:
                    PopulateEntityListFull();
                    break;
            }
        }

        /// <summary>
        /// Assigns new entity UIDs that differ from the ones in the blueprint.
        /// </summary>
        public void PopulateEntityListBlueprint()
        {
            var withUid = new List<MapSaveIdComponent>();
            var withoutUid = new List<EntityUid>();
            var takenIds = new HashSet<int>();

            foreach (var entityUid in GetAllEntitiesInMap(_targetMapId))
            {
                if (IsMapSavable(entityUid))
                {
                    _context.Entities.Add(entityUid);
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
                    withoutUid.Add(mapIdComp.Owner);
                }
                else
                {
                    _context.EntityUidMap.Add(mapIdComp.Owner, uid);
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

                _context.EntityUidMap.Add(entity, uidCounter);
                takenIds.Add(uidCounter);
            }
        }

        /// <summary>
        /// Assigns UIDs that are the same as those in the saved map. This is needed
        /// for game saves.
        /// </summary>
        private void PopulateEntityListFull()
        {
            foreach (var entityUid in GetAllEntitiesInMap(_targetMapId))
            {
                if (IsMapSavable(entityUid))
                {
                    _context.Entities.Add(entityUid);
                    _context.EntityUidMap.Add(entityUid, (int)entityUid);
                }
            }
        }

        /// <summary>
        /// { entityPrototypeId -> { compName, prototypeCompData } ]
        /// </summary>
        private class PrototypeCompCache : Dictionary<string, Dictionary<string, MappingDataNode>>
        {
        }

        private void WriteEntitySection()
        {
            var entities = new YamlSequenceNode();
            _rootNode.Add(MapLoadConstants.Entities, entities);

            var prototypeCompCache = new PrototypeCompCache();
            foreach (var entity in _context.Entities.OrderBy(e => _context.EntityUidMap[e]))
            {
                var mapping = SerializeEntity(entity, prototypeCompCache);
                entities.Add(mapping);
            }
        }

        private YamlMappingNode SerializeEntity(EntityUid entity, PrototypeCompCache prototypeCompCache)
        {
            var mapping = new YamlMappingNode
            {
                {MapLoadConstants.Entities_Uid, _context.EntityUidMap[entity].ToString(CultureInfo.InvariantCulture)}
            };

            if (_entityManager.GetComponent<MetaDataComponent>(entity).EntityPrototype is { } prototype)
            {
                mapping.Add(MapLoadConstants.Entities_ProtoId, prototype.ID);
                if (!prototypeCompCache.ContainsKey(prototype.ID))
                {
                    prototypeCompCache[prototype.ID] = new Dictionary<string, MappingDataNode>();
                    foreach (var (compType, comp) in prototype.Components)
                    {
                        prototypeCompCache[prototype.ID].Add(compType, _serializationManager.WriteValueAs<MappingDataNode>(comp.GetType(), comp));
                    }
                }
            }

            YamlSequenceNode components;

            switch (_mode)
            {
                case MapSerializeMode.Blueprint:
                    components = SerializeComponentsPartial(entity, prototypeCompCache);
                    break;
                case MapSerializeMode.Full:
                default:
                    components = SerializeComponentsFull(entity);
                    break;
            }

            if (components.Children.Count != 0)
            {
                mapping.Add(MapLoadConstants.Entities_Components, components);
            }

            return mapping;
        }

        /// <summary>
        /// Serializes the exact list of components on this entity, accounting for addded/deleted 
        /// components that differ from the prototype. There is no compression accounting for
        /// unchanged prototype fields with this method.
        /// </summary>
        private YamlSequenceNode SerializeComponentsFull(EntityUid entity)
        {
            var components = new YamlSequenceNode();

            foreach (var component in _entityManager.GetComponents(entity))
            {
                if (component is MapSaveIdComponent)
                    continue;

                var compMapping = _serializationManager.WriteValueAs<MappingDataNode>(component.GetType(), component, context: _context);

                if (compMapping == null)
                {
                    // Still want a '- type: XXX' node for tracking deletions
                    compMapping = new MappingDataNode();
                }

                compMapping.Add(MapLoadConstants.Entities_Components_Type, new ValueDataNode(component.Name));
                components.Add(compMapping.ToYamlNode());
            }

            return components;
        }

        /// <summary>
        /// Saves the entity's components, omitting components that are completely the same
        /// as the prototype's.
        /// </summary>
        private YamlSequenceNode SerializeComponentsPartial(EntityUid entity, PrototypeCompCache prototypeCompCache)
        {
            var components = new YamlSequenceNode();

            foreach (var component in _entityManager.GetComponents(entity))
            {
                if (component is MapSaveIdComponent)
                    continue;

                var compMapping = _serializationManager.WriteValueAs<MappingDataNode>(component.GetType(), component, context: _context);

                var md = _entityManager.GetComponent<MetaDataComponent>(entity);
                if (md.EntityPrototype != null && prototypeCompCache[md.EntityPrototype.ID].TryGetValue(component.Name, out var protMapping))
                {
                    // NOTE: I think this is buggy. Except() seems to not work well with complex mapping nodes
                    // and dynamically instantiated types. (Sometimes the the constructor default gets output instead of
                    // the prototype's specified value.)
                    //
                    // If map blueprint serialization becomes desirable, this *will* need to be revisited.
                    compMapping = compMapping.Except(protMapping);
                    if (compMapping == null) continue;
                }

                // Don't need to write it if nothing was written!
                if (compMapping.Children.Count != 0)
                {
                    compMapping.Add(MapLoadConstants.Entities_Components_Type, new ValueDataNode(component.Name));
                    // Something actually got written!
                    components.Add(compMapping.ToYamlNode());
                }
            }

            return components;
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
                if (_entityManager.GetComponent<MetaDataComponent>(parent.Owner).EntityPrototype?.MapSavable == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
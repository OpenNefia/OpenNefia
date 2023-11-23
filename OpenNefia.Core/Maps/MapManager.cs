using OpenNefia.Analyzers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.Maps
{
    internal interface IMapManagerInternal : IMapManager
    { 
        /// <summary>
        /// The next free map ID to use when generating new maps.
        /// </summary>
        /// <remarks>
        /// This should **only** be set when handling game saving/loading.
        /// </remarks>
        int NextMapId { get; set; }

        /// <summary>
        /// Registers a map loaded from a save.
        /// </summary>
        /// <remarks>
        /// Do **NOT** use this function to manually register new maps! This bypasses 
        /// the tracking for free map IDs, since the assumption is that the passed map
        /// previously existed in memory at some point but was unloaded.
        /// </remarks>
        /// <param name="map">The map loaded from a save.</param>
        /// <param name="mapId">The map ID this map was registered with at the time of saving.</param>
        /// <param name="mapEntityUid">The map entity UID to associate with this map, also loaded from a save.</param>
        void RegisterMap(IMap map, MapId mapId, EntityUid mapEntityUid);

        /// <summary>
        /// Clears the active map list.
        /// </summary>
        /// <remarks>
        /// This should **only** be called when handling game saving/loading.
        /// </remarks>
        void FlushMaps();
    }

    public sealed partial class MapManager : IMapManagerInternal
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private protected readonly Dictionary<MapId, IMap> _maps = new();
        private protected readonly Dictionary<MapId, EntityUid> _mapEntities = new();

        /// <inheritdoc/>
        public event ActiveMapChangedDelegate? OnActiveMapChanged;

        /// <inheritdoc/>
        public IMap? ActiveMap { get; private set; } = null;

        /// <inheritdoc/>
        public IReadOnlyDictionary<MapId, IMap> LoadedMaps => _maps;

        /// <inheritdoc/>
        public int NextMapId { get; set; } = (int)MapId.FirstId;

        /// <inheritdoc/>
        public MapId GenerateMapId()
        {
            return new(NextMapId++);
        }

        /// <inheritdoc/>
        public bool MapIsLoaded(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }

        /// <inheritdoc/>
        public void FlushMaps()
        {
            _maps.Clear();
            _mapEntities.Clear();
            ActiveMap = null;
            NextMapId = (int)MapId.FirstId;
        }

        /// <inheritdoc/>
        public IMap CreateMap(int width, int height, PrototypeId<EntityPrototype>? mapEntityProto = null)
        {
            return CreateMap(width, height, GenerateMapId(), mapEntityProto);  
        }

        /// <inheritdoc/>
        public IMap CreateMap(int width, int height, MapId mapId, PrototypeId<EntityPrototype>? mapEntityProto = null)
        {
            if (MapIsLoaded(mapId))
                throw new ArgumentException($"Attempted to create an already existing map {mapId}!");

            var map = new Map(width, height);
            _maps.Add(mapId, map);
            map.Id = mapId;
            RebindMapEntity(mapId, map, mapEntityProto);

            return map;
        }

        private static void SetMapAndEntityIds(IMap map, MapId mapId, EntityUid mapEntityUid)
        {
            var mapInternal = (Map)map;
            mapInternal.Id = mapId;
            mapInternal.MapEntityUid = mapEntityUid;
        }

        /// <inheritdoc/>
        public void RegisterMap(IMap map, MapId mapId, EntityUid mapEntityUid)
        {
            if (mapId == MapId.Nullspace)
            {
                throw new ArgumentException("Can't register null map.", nameof(mapId));
            }
            if (!_entityManager.EntityExists(mapEntityUid))
            {
                throw new ArgumentException($"Map entity {mapEntityUid} doesn't exist.", nameof(mapEntityUid));
            }
            // Check to see if the IDs on the passed map were already set elsewhere.
            if (map.MapEntityUid.IsValid() || map.Id != MapId.Nullspace)
            {
                throw new ArgumentException("Map is already in use.", nameof(map)); 
            }

            _maps[mapId] = map;

            SetMapEntity(mapId, mapEntityUid);
            SetMapAndEntityIds(map, mapId, mapEntityUid);
        }

        private EntityUid RebindMapEntity(MapId actualID, IMap map, PrototypeId<EntityPrototype>? mapEntityProto = null)
        {
            var mapComps = _entityManager.EntityQuery<MapComponent>();

            MapComponent? result = null;
            foreach (var mapComp in mapComps)
            {
                if (mapComp.MapId != actualID)
                    continue;

                result = mapComp;
                break;
            }

            if (result != null)
            {
                _mapEntities.Add(actualID, result.Owner);
                Logger.DebugS("map", $"Rebinding map {actualID} to entity {result.Owner}");
                return result.Owner;
            }
            else
            {
                var newEnt = _entityManager.CreateEntityUninitialized(mapEntityProto);
                _mapEntities.Add(actualID, newEnt);
                
                // Make sure the map IDs are set on the map object before map component
                // events are fired.
                SetMapAndEntityIds(map, actualID, newEnt);

                var mapComp = _entityManager.EnsureComponent<MapComponent>(newEnt);
                mapComp.MapId = actualID;

                _entityManager.InitializeComponents(newEnt);
                _entityManager.StartComponents(newEnt);
                Logger.DebugS("map", $"Binding map {actualID} to entity {newEnt}");

                var ev = new MapCreatedEvent(map);
                _entityManager.EventBus.RaiseEvent(map.MapEntityUid, ev);

                return newEnt;
            }
        }

        /// <inheritdoc/>
        public void SetMapEntity(MapId mapId, EntityUid newMapEntity)
        {
            if (!_maps.TryGetValue(mapId, out var mapGrid))
                throw new InvalidOperationException($"Map {mapId} does not exist.");

            foreach (var kvEntity in _mapEntities)
            {
                if (kvEntity.Value == newMapEntity)
                {
                    throw new InvalidOperationException(
                        $"Entity {newMapEntity} is already the root node of map {kvEntity.Key}.");
                }
            }

            // remove existing graph
            if (_mapEntities.TryGetValue(mapId, out var oldEntId))
            {
                //Note: This prevents setting a subgraph as the root, since the subgraph will be deleted
                _entityManager.DeleteEntity(oldEntId);
            }
            else
            {
                _mapEntities.Add(mapId, EntityUid.Invalid);
            }

            // re-use or add map component
            if (!_entityManager.TryGetComponent(newMapEntity, out MapComponent? mapComp))
            {
                mapComp = _entityManager.AddComponent<MapComponent>(newMapEntity);
            }
            else
            {
                if (mapComp.MapId != mapId)
                    Logger.WarningS("map",
                        $"Setting map {mapId} root to entity {newMapEntity}, but entity thinks it is root node of map {mapComp.MapId}.");
            }

            Logger.DebugS("map", $"Setting map {mapId} entity to {newMapEntity}");

            // set as new map entity
            mapComp.MapId = mapId;
            _mapEntities[mapId] = newMapEntity;
            SetMapAndEntityIds(mapGrid, mapId, newMapEntity);
        }

        /// <inheritdoc/>
        public void UnloadMap(MapId mapID, MapUnloadType unloadType = MapUnloadType.Unload)
        {
            if (mapID == ActiveMap?.Id)
            {
                ActiveMap = null;
            }

            if (!_maps.TryGetValue(mapID, out var map))
            {
                Logger.DebugS("map", $"Attempted to unload nonexistent map '{mapID}'");
                return;
            }

            Logger.InfoS("map", $"Unloading map {mapID}.");

            EntityDeleteType entityDeleteType;

            if (unloadType == MapUnloadType.Unload)
            {
                var ev = new BeforeMapUnloadedEvent(map);
                entityDeleteType = EntityDeleteType.Unload;
                _entityManager.EventBus.RaiseEvent(map.MapEntityUid, ev);
            }
            else
            {
                var ev = new BeforeMapDeletedEvent(map);
                entityDeleteType = EntityDeleteType.Delete;
                _entityManager.EventBus.RaiseEvent(map.MapEntityUid, ev);
            }

            _maps.Remove(mapID);
            UnloadEntitiesInMap(mapID);

            var mapEnt = _mapEntities[mapID];
            _entityManager.DeleteEntity(mapEnt, entityDeleteType);
            _mapEntities.Remove(mapID);
        }

        private void UnloadEntitiesInMap(MapId mapID)
        {
            foreach (var spatial in _entityManager.GetAllComponents<SpatialComponent>().ToList())
            {
                if (spatial.MapID == mapID)
                {
                    var meta = _entityManager.GetComponent<MetaDataComponent>(spatial.Owner);
                    var deleteType = meta.IsMapSavable ? EntityDeleteType.Delete : EntityDeleteType.Unload;

                    _entityManager.DeleteEntity(spatial.Owner, deleteType);
                }
            }
        }

        /// <inheritdoc/>
        public IMap GetMap(MapId mapId)
        {
            return _maps[mapId];
        }

        /// <inheritdoc/>
        public IMap GetMapOfEntity(EntityUid entity)
        {
            var spatial = _entityManager.GetComponent<SpatialComponent>(entity);
            return GetMap(spatial.MapID);
        }

        /// <inheritdoc/>
        public bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map)
        {
            return _maps.TryGetValue(mapId, out map);
        }

        /// <inheritdoc/>
        public bool TryGetMapOfEntity(EntityUid entity, [NotNullWhen(true)] out IMap? map)
        {
            if (!_entityManager.TryGetComponent<SpatialComponent>(entity, out var spatial))
            {
                map = null;
                return false;
            }
            
            return TryGetMap(spatial.MapID, out map);
        }

        /// <inheritdoc/>
        public void SetActiveMap(MapId mapId, MapLoadType loadType = MapLoadType.Full)
        {
            if (mapId == MapId.Global)
                throw new ArgumentException($"Cannot set the active map to the global map.", nameof(mapId));

            if (mapId == ActiveMap?.Id)
                return;

            if (!_maps.ContainsKey(mapId))
            {
                throw new ArgumentException($"Cannot find map {mapId}!", nameof(mapId));
            }

            var oldMap = ActiveMap;
            var map = _maps[mapId];
            ActiveMap = map;
            OnActiveMapChanged?.Invoke(map, oldMap, loadType);
        }

        /// <inheritdoc/>
        public bool IsMapInitialized(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }

        /// <inheritdoc/>
        public void RefreshVisibility(IMap map)
        {
            map.LastSightId++;

            var ev = new RefreshMapVisibilityEvent(map);
            IoCManager.Resolve<IEntityManager>().EventBus.RaiseEvent(map.MapEntityUid, ref ev);

            var outOfSightCoords = map.MapObjectMemory.AllMemory.Values
                .Where(memory => memory.HideWhenOutOfSight && !map.IsInWindowFov(memory.Coords.Position))
                .Select(memory => memory.Coords)
                .Distinct();

            foreach (var coords in outOfSightCoords)
            {
                map.MapObjectMemory.HideObjects(coords.Position);
            }
        }
    }

    /// <summary>
    /// Raised when a map is created from scratch.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public sealed class MapCreatedEvent : EntityEventArgs
    {
        public IMap Map { get; }

        public MapCreatedEvent(IMap map)
        {
            Map = map;
        }
    }

    public enum MapCreationMode
    {
        Normal,
        LoadedFromBlueprint,
    }

    /// <summary>
    /// Raised when a map is loaded from save data.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public sealed class MapLoadedFromSaveEvent : EntityEventArgs
    {
        public IMap Map { get; }

        public MapLoadedFromSaveEvent(IMap map)
        {
            Map = map;
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class BeforeMapUnloadedEvent : EntityEventArgs
    {
        public IMap Map { get; }

        public BeforeMapUnloadedEvent(IMap map)
        {
            Map = map;
        }
    }

    [EventUsage(EventTarget.Map)]
    public sealed class BeforeMapDeletedEvent : EntityEventArgs
    {
        public IMap Map { get; }

        public BeforeMapDeletedEvent(IMap map)
        {
            Map = map;
        }
    }

    [ByRefEvent]
    public struct RefreshMapVisibilityEvent
    {
        public IMap Map { get; }

        public RefreshMapVisibilityEvent(IMap map)
        {
            Map = map;
        }
    }
}

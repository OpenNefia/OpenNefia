using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.SaveGames;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.Maps
{
    public interface IMapManagerInternal : IMapManager
    { 
        /// <summary>
        /// The next free map ID to use when generating new maps.
        /// </summary>
        /// <remarks>
        /// This should **only** be set when handling game saving/loading.
        /// </remarks>
        MapId HighestMapId { get; set; }

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
        public event ActiveMapChangedDelegate? ActiveMapChanged;

        /// <inheritdoc/>
        public IMap? ActiveMap { get; private set; } = null;

        /// <inheritdoc/>
        public MapId HighestMapId { get; set; } = MapId.Nullspace;

        /// <inheritdoc/>
        public bool MapIsLoaded(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }

        /// <inheritdoc/>
        public MapId GetFreeMapIdAndIncrement()
        {
            var newId = new MapId(HighestMapId.Value + 1);
            HighestMapId = newId;
            return newId;
        }

        /// <inheritdoc/>
        public void FlushMaps()
        {
            _maps.Clear();
            _mapEntities.Clear();
            ActiveMap = null;
            HighestMapId = MapId.Nullspace;
        }

        /// <inheritdoc/>
        public IMap CreateMap(int width, int height)
        {
            var actualID = GetFreeMapIdAndIncrement();

            var map = new Map(width, height);
            _maps.Add(actualID, map);
            map.Id = actualID;
            RebindMapEntity(actualID, map);

            return map;
        }

        internal static void SetMapGridIds(IMap map, MapId mapId, EntityUid mapEntityUid)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var idField = map.GetType().GetProperty("Id", flags)!;
            var mapEntityUidField = map.GetType().GetProperty("MapEntityUid", flags)!;
            idField.SetValue(map, mapId);
            mapEntityUidField.SetValue(map, mapEntityUid);
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
            //HighestMapId = 99999;
        }

        private EntityUid RebindMapEntity(MapId actualID, IMap map)
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
                _mapEntities.Add(actualID, result.OwnerUid);
                Logger.DebugS("map", $"Rebinding map {actualID} to entity {result.OwnerUid}");
                return result.OwnerUid;
            }
            else
            {
                var newEnt = _entityManager.CreateEntityUninitialized(null);
                _mapEntities.Add(actualID, newEnt.Uid);
                
                // Make sure the map IDs are set on the map object before map component
                // events are fired.
                SetMapGridIds(map, actualID, newEnt.Uid);

                var mapComp = newEnt.AddComponent<MapComponent>();
                mapComp.MapId = actualID;

                _entityManager.InitializeComponents(newEnt.Uid);
                _entityManager.StartComponents(newEnt.Uid);
                Logger.DebugS("map", $"Binding map {actualID} to entity {newEnt.Uid}");
                return newEnt.Uid;
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
            SetMapGridIds(mapGrid, mapId, newMapEntity);
        }

        /// <inheritdoc/>
        public void UnloadMap(MapId mapID)
        {
            if (mapID == ActiveMap?.Id)
            {
                ActiveMap = null;
            }

            if (!_maps.ContainsKey(mapID))
            {
                throw new InvalidOperationException($"Attempted to delete nonexistant map '{mapID}'");
            }

            Logger.InfoS("map", $"Unloading map {mapID}");

            _maps.Remove(mapID);
            UnloadEntitiesInMap(mapID);

            var mapEnt = _mapEntities[mapID];
            _entityManager.DeleteEntity(mapEnt);
            _mapEntities.Remove(mapID);
        }

        private void UnloadEntitiesInMap(MapId mapID)
        {
            foreach (var entity in _entityManager.GetEntities().ToList())
            {
                if (entity.Spatial.MapID == mapID)
                {
                    _entityManager.DeleteEntity(entity.Uid);
                }
            }
        }

        /// <inheritdoc/>
        public IMap GetMap(MapId mapId)
        {
            return _maps[mapId];
        }

        /// <inheritdoc/>
        public Entity GetMapEntity(MapId mapId)
        {
            if (!_mapEntities.ContainsKey(mapId))
                throw new InvalidOperationException($"Map {mapId} does not have a set map entity.");

            return _entityManager.GetEntity(_mapEntities[mapId]);
        }

        /// <inheritdoc/>
        public bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map)
        {
            return _maps.TryGetValue(mapId, out map);
        }

        /// <inheritdoc/>
        public bool TryGetMap(EntityUid mapEntityUid, [NotNullWhen(true)] out IMap? map)
        {
            if (!_entityManager.TryGetComponent(mapEntityUid, out MapComponent? mapComp))
            {
                map = null;
                return false;
            }

            return TryGetMap(mapComp.MapId, out map);
        }

        /// <inheritdoc/>
        public bool TryGetMapEntity(MapId mapId, [NotNullWhen(true)] out Entity? mapEntity)
        {
            mapEntity = null;

            if (!_mapEntities.TryGetValue(mapId, out var mapEntityUid))
                return false;

            return _entityManager.TryGetEntity(mapEntityUid, out mapEntity);
        }

        /// <inheritdoc/>
        public void SetActiveMap(MapId mapId)
        {
            if (mapId == ActiveMap?.Id)
                return;

            if (!_maps.ContainsKey(mapId))
            {
                throw new ArgumentException($"Cannot find map {mapId}!", nameof(mapId));
            }

            var oldMap = ActiveMap;
            var map = _maps[mapId];
            ActiveMap = map;
            ActiveMapChanged?.Invoke(map, oldMap);
        }

        /// <inheritdoc/>
        public bool IsMapInitialized(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }
    }
}

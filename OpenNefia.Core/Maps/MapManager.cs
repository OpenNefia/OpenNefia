using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace OpenNefia.Core.Maps
{
    public class MapManager : IMapManager
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private protected readonly Dictionary<MapId, IMap> _maps = new();
        private protected readonly Dictionary<MapId, EntityUid> _mapEntities = new();

        public event ActiveMapChangedDelegate? ActiveMapChanged;

        public IMap? ActiveMap { get; private set; } = default!;

        private MapId _highestMapID = MapId.Nullspace;

        public bool MapExists(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }

        public MapId GetFreeMapId()
        {
            return new MapId(_highestMapID.Value + 1);
        }

        public IMap CreateMap(int width, int height, MapId? mapId = null)
        {
            var actualID = AllocFreeMapId(mapId);

            var map = new Map(width, height);
            this._maps[_highestMapID] = map;
            map.Id = actualID;
            map.MapEntityUid = RebindMapEntity(actualID);

            return map;
        }

        private void SetMapGridIds(IMap map, MapId mapId, EntityUid mapEntityUid)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var idField = map.GetType().GetProperty("Id", flags)!;
            var mapEntityUidField = map.GetType().GetProperty("MapEntityUid", flags)!;
            idField.SetValue(map, mapId);
            mapEntityUidField.SetValue(map, mapEntityUid);
        }

        public MapId RegisterMap(IMap map, MapId? mapId = null, EntityUid? mapEntityUid = null)
        {
            var actualID = AllocFreeMapId(mapId);

            if (map.MapEntityUid.IsValid() || map.Id != MapId.Nullspace)
            {
                throw new ArgumentException("Map is already in use.", nameof(map));
            }

            this._maps[_highestMapID] = map;

            if (mapEntityUid == null)
            {
                mapEntityUid = RebindMapEntity(actualID);
            }
            else
            {
                SetMapEntity(actualID, mapEntityUid.Value);
            }

            SetMapGridIds(map, actualID, mapEntityUid.Value);

            return actualID;
        }

        private MapId AllocFreeMapId(MapId? mapId)
        {
            if (mapId == MapId.Nullspace)
            {
                throw new ArgumentException("Null map cannot be created.", nameof(mapId));
            }

            MapId actualID;
            if (mapId != null)
            {
                actualID = mapId.Value;
            }
            else
            {
                actualID = GetFreeMapId();
            }

            if (MapExists(actualID))
            {
                throw new InvalidOperationException($"A map with ID {actualID} already exists");
            }

            if (_highestMapID.Value < actualID.Value)
            {
                _highestMapID = actualID;
            }

            return actualID;
        }

        private EntityUid RebindMapEntity(MapId actualID)
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

                var mapComp = newEnt.AddComponent<MapComponent>();
                mapComp.MapId = actualID;
                _entityManager.InitializeComponents(newEnt.Uid);
                _entityManager.StartComponents(newEnt.Uid);
                Logger.DebugS("map", $"Binding map {actualID} to entity {newEnt.Uid}");
                return newEnt.Uid;
            }
        }

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

            _maps.Remove(mapID);

            if (_mapEntities.TryGetValue(mapID, out var ent))
            {
                if (_entityManager.TryGetEntity(ent, out var mapEnt))
                    mapEnt.Delete();

                _mapEntities.Remove(mapID);
            }

            Logger.InfoS("map", $"Deleting map {mapID}");
        }

        public IMap GetMap(MapId mapId)
        {
            return _maps[mapId];
        }

        public Entity GetMapEntity(MapId mapId)
        {
            if (!_mapEntities.ContainsKey(mapId))
                throw new InvalidOperationException($"Map {mapId} does not have a set map entity.");

            return _entityManager.GetEntity(_mapEntities[mapId]);
        }

        public bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map)
        {
            return _maps.TryGetValue(mapId, out map);
        }

        public bool TryGetMapEntity(MapId mapId, [NotNullWhen(true)] out Entity? mapEntity)
        {
            mapEntity = null;

            if (!_mapEntities.TryGetValue(mapId, out var mapEntityUid))
                return false;

            return _entityManager.TryGetEntity(mapEntityUid, out mapEntity);
        }

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

        public bool IsMapInitialized(MapId mapId)
        {
            return _maps.ContainsKey(mapId);
        }
    }
}

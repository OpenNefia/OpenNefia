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

        public MapId RegisterMap(IMap map, MapId? mapId = null)
        {
            var actualID = AllocFreeMapId(mapId);

            if (map.MapEntityUid.IsValid() || map.Id != MapId.Nullspace)
            {
                throw new ArgumentException("Map is already in use.", nameof(map));
            }

            this._maps[_highestMapID] = map;

            var idField = map.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
            var mapEntityUidField = map.GetType().GetProperty("MapEntityUid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
            idField.SetValue(map, actualID);
            mapEntityUidField.SetValue(map, RebindMapEntity(actualID));

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

        public void ChangeActiveMap(MapId mapId)
        {
            this.ActiveMap = _maps[mapId];
            _highestMapID = mapId;
        }

        public bool IsMapInitialized(MapId mapId)
        {
            return this._maps.ContainsKey(mapId);
        }
    }
}

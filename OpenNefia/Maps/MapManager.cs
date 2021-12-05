using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public class MapManager : IMapManager
    {
        public Dictionary<MapId, IMap> CachedMaps = new Dictionary<MapId, IMap>();

        public IMap? ActiveMap { get; private set; } = default!;

        private MapId _highestMapID = MapId.Nullspace;

        public bool MapExists(MapId mapId)
        {
            return CachedMaps.ContainsKey(mapId);
        }

        public MapId CreateMap(MapId? mapId, int width, int height)
        {
            MapId actualID;
            if (mapId != null)
            {
                actualID = mapId.Value;
            }
            else
            {
                actualID = new MapId(_highestMapID.Value + 1);
            }

            if (MapExists(actualID))
            {
                throw new InvalidOperationException($"A map with ID {actualID} already exists");
            }

            if (_highestMapID.Value < actualID.Value)
            {
                _highestMapID = actualID;
            }

            var map = new Map(width, height);
            this.CachedMaps[_highestMapID] = map;
            map.Id = actualID;

            return actualID;
        }

        public IMap LoadMap(MapId mapId)
        {
            if (CachedMaps.TryGetValue(mapId, out IMap? map))
                return map;

            map = new Map(100, 100);
            CachedMaps.Add(mapId, map);
            return map;
        }

        public void UnloadMap(MapId mapId)
        {
            CachedMaps.Remove(mapId);

            if (mapId == ActiveMap?.Id)
            {
                ActiveMap = null;
            }
        }

        public void SaveMap(MapId mapId)
        {

        }

        public MapId RegisterMap(IMap map)
        {
            DebugTools.Assert(map.Id == MapId.Nullspace, $"Map {map.Id} already registered");
            _highestMapID = new MapId(_highestMapID.Value + 1);
            map.Id = _highestMapID;
            this.CachedMaps[_highestMapID] = map;
            return map.Id;
        }

        public IMap GetMap(MapId mapId)
        {
            return CachedMaps[mapId];
        }

        public IMap? GetMapOrNull(MapId mapId)
        {
            return CachedMaps.GetValueOrDefault(mapId);
        }

        public void ChangeActiveMap(MapId mapId)
        {
            this.ActiveMap = CachedMaps[mapId];
            _highestMapID = mapId;
        }

        public bool IsMapInitialized(MapId mapId)
        {
            return this.CachedMaps.ContainsKey(mapId);
        }

        public IEnumerable<Entity> GetAllEntities(MapId mapId)
        {
            return GetMapOrNull(mapId)
                ?.Entities
                ?? Enumerable.Empty<Entity>();
        }

        /// <inheritdoc />
        public IEnumerable<Entity> GetLiveEntities(MapCoordinates coords)
        {
            return coords.Map
                 ?.Entities
                 .Where(entity => entity.Spatial.Coords == coords
                               && entity.MetaData.IsAlive)
                 ?? Enumerable.Empty<Entity>();
        }
    }
}

using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public class MapManager : IMapManager
    {
        public Dictionary<MapId, IMap> CachedMaps = new Dictionary<MapId, IMap>();

        public IMap? ActiveMap { get; private set; } = default!;

        private MapId _currentMapId = MapId.Nullspace;

        public IMap LoadMap(MapId id)
        {
            if (CachedMaps.TryGetValue(id, out IMap? map))
                return map;

            map = new Map(100, 100);
            CachedMaps.Add(id, map);
            return map;
        }

        public void UnloadMap(MapId id)
        {
            CachedMaps.Remove(id);

            if (id == _currentMapId)
            {
                _currentMapId = MapId.Nullspace;
                ActiveMap = null;
            }
        }

        public void SaveMap(MapId id)
        {

        }

        public MapId RegisterMap(IMap map)
        {
            DebugTools.Assert(map.Id == MapId.Nullspace, $"Map {map.Id} already registered");
            _currentMapId = new MapId(_currentMapId.Value + 1);
            map.Id = _currentMapId;
            this.CachedMaps[_currentMapId] = map;
            return map.Id;
        }

        public IMap GetMap(MapId id)
        {
            return CachedMaps[id];
        }

        public IMap? GetMapOrNull(MapId id)
        {
            return CachedMaps.GetValueOrDefault(id);
        }

        public void ChangeActiveMap(MapId id)
        {
            this.ActiveMap = CachedMaps[id];
            _currentMapId = id;
        }

        public bool IsMapInitialized(MapId mapId)
        {
            return this.CachedMaps.ContainsKey(mapId);
        }

        public IEnumerable<Entity> GetEntities(MapCoordinates coords)
        {
            return coords.Map
                 ?.Entities
                 .Where(entity => entity.Spatial.Coords == coords)
                 ?? Enumerable.Empty<Entity>();
        }
    }
}

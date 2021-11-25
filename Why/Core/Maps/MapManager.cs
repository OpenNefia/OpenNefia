using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Utility;

namespace Why.Core.Maps
{
    public class MapManager : IMapManager
    {
        public Dictionary<MapId, IMap> CachedMaps = new Dictionary<MapId, IMap>();

        public IMap CurrentMap = default!;

        private MapId _currentMapId = MapId.Nullspace;

        public IMap LoadMap(MapId id)
        {
            if (CachedMaps.TryGetValue(id, out IMap? map))
                return map;

            map = new Map(100, 100);
            CachedMaps.Add(id, map);
            return map;
        }

        public MapId RegisterMap(IMap map)
        {
            DebugTools.Assert(map.Id == MapId.Nullspace, $"Map {map.Id} already registered");
            _currentMapId = new MapId(_currentMapId.Value + 1);
            map.Id = _currentMapId;
            this.CachedMaps[_currentMapId] = map;
            return map.Id;
        }

        public void SaveMap(MapId id)
        {

        }

        public void ChangeCurrentMap(MapId id)
        {
            this.CurrentMap = CachedMaps[id];
        }

        public bool IsMapInitialized(MapId mapId)
        {
            return this.CachedMaps.ContainsKey(mapId);
        }
    }
}

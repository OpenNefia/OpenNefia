using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    public sealed partial class AreaManager
    {
        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, IMap map)
            => RegisterAreaFloor(area, floorId, map.Id);

        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, MapId mapId)
            => RegisterAreaFloor(area, floorId, new AreaFloor(mapId));

        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, AreaFloor floor)
        {
            if (area.ContainedMaps.ContainsKey(floorId))
            {
                throw new ArgumentException($"Area {area.Id} already has a floor with ID {floorId} registered.", nameof(area));
            }
            if (floor.MapId.HasValue && !_mapManager.MapIsLoaded(floor.MapId.Value)) 
            {
                Logger.WarningS("area", $"Map {floor.MapId} not loaded when registering it with area {area.Id} at floor {floorId}");
            } 

            var areaInternal = ((Area)area);
            areaInternal._containedMaps.Add(floorId, floor);
        }

        public void UnregisterAreaFloor(IArea area, AreaFloorId floorId)
        {
            if (!area.ContainedMaps.ContainsKey(floorId))
            {
                throw new ArgumentException($"Area {area.Id} does not have a floor with ID {floorId} registered.", nameof(area));
            }

            var areaInternal = ((Area)area);
            areaInternal._containedMaps.Remove(floorId);
        }
    }
}

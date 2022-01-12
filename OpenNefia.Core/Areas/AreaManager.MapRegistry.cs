using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Areas
{
    public sealed partial class AreaManager
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;

        /// <inheritdoc/>
        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, IMap map)
            => RegisterAreaFloor(area, floorId, map.Id);

        /// <inheritdoc/>
        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, MapId mapId)
            => RegisterAreaFloor(area, floorId, new AreaFloor(mapId));

        /// <inheritdoc/>
        public void RegisterAreaFloor(IArea area, AreaFloorId floorId, AreaFloor floor)
        {
            if (area.ContainedMaps.ContainsKey(floorId))
            {
                throw new ArgumentException($"Area {area.Id} already has a floor with ID {floorId} registered.", nameof(area));
            }
            if (floor.MapId.HasValue)
            {
                if (_mapsToAreas.TryGetValue(floor.MapId.Value, out var pair))
                {
                    throw new ArgumentException($"Map {floor.MapId.Value} is already registered with area {pair.Item1.Id} on floor {pair.Item2}.", nameof(floor));
                }
                if (!_mapManager.MapIsLoaded(floor.MapId.Value))
                {
                    Logger.WarningS("area", $"Map {floor.MapId} not loaded when registering it with area {area.Id} at floor {floorId}");
                }
            }

            var areaInternal = ((Area)area);
            areaInternal._containedMaps.Add(floorId, floor);
            if (floor.MapId != null)
                _mapsToAreas.Add(floor.MapId.Value, (area, floorId));
        }

        /// <inheritdoc/>
        public void UnregisterAreaFloor(IArea area, AreaFloorId floorId)
        {
            if (!area.ContainedMaps.TryGetValue(floorId, out var floor))
            {
                throw new ArgumentException($"Area {area.Id} does not have a floor with ID {floorId} registered.", nameof(area));
            }

            var areaInternal = ((Area)area);
            areaInternal._containedMaps.Remove(floorId);
            if (floor.MapId != null)
                _mapsToAreas.Remove(floor.MapId.Value);
        }

        /// <inheritdoc/>
        public bool TryGetAreaAndFloorOfMap(MapId map, [NotNullWhen(true)] out IArea? area, [NotNullWhen(true)] out AreaFloorId floorId)
        {
            area = null;
            floorId = default;

            if (!_mapsToAreas.TryGetValue(map, out var pair))
                return false;

            area = pair.Item1;
            floorId = pair.Item2;
            return true;
        }

        /// <inheritdoc/>
        public bool TryGetAreaOfMap(MapId map, [NotNullWhen(true)] out IArea? area)
        {
            return TryGetAreaAndFloorOfMap(map, out area, out _);
        }

        // TODO: This is probably going into an IMapGenerator interface later.
        public MapId GenerateMapForFloor(AreaId areaId, AreaFloorId floorId)
        {
            var area = GetArea(areaId);
            var floor = area.ContainedMaps[floorId];
            if (floor.MapId != null)
            {
                Logger.WarningS("area", $"Area/floor '{areaId}/'{floorId}' has already been generated, reusing generated map {floor.MapId}.");
                return floor.MapId.Value;
            }
            var proto = _prototypeManager.Index(floor.DefaultGenerator);
            var mapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;
            floor.MapId = mapId;
            _mapsToAreas.Add(mapId, (area, floorId));
            return mapId;
        }
    }
}

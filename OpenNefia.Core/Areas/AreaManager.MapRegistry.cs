using OpenNefia.Core.GameObjects;
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
        public bool TryGetAreaOfMap(IMap map, [NotNullWhen(true)] out IArea? area)
            => TryGetAreaOfMap(map.Id, out area);

        /// <inheritdoc/>
        public bool TryGetAreaOfMap(MapId map, [NotNullWhen(true)] out IArea? area)
        {
            return TryGetAreaAndFloorOfMap(map, out area, out _);
        }

        public MapId? GenerateMapForFloor(AreaId areaId, AreaFloorId floorId)
        {
            var area = GetArea(areaId);
            var floor = area.ContainedMaps[floorId];
            if (floor.MapId != null)
            {
                Logger.WarningS("area", $"Area/floor '{areaId}/'{floorId}' has already been generated, reusing generated map {floor.MapId}.");
                return floor.MapId.Value;
            }

            var ev = new AreaFloorGenerateEvent(area, floorId);
            _entityManager.EventBus.RaiseLocalEvent(area.AreaEntityUid, ev);

            if (ev.ResultMapId == null)
            {
                Logger.ErrorS("area", $"Failed to generate map for area/floor '{areaId}'/'{floorId}'!");
                return null;
            }

            floor.MapId = ev.ResultMapId.Value;
            _mapsToAreas.Add(floor.MapId.Value, (area, floorId));

            return floor.MapId;
        }
    }

    /// <summary>
    /// Raised when a new floor in a area needs to be generated.
    /// </summary>
    public sealed class AreaFloorGenerateEvent : HandledEntityEventArgs
    {
        public IArea Area { get; }
        public AreaFloorId FloorId { get; }

        public MapId? ResultMapId { get; private set; }

        public AreaFloorGenerateEvent(IArea area, AreaFloorId floorId)
        {
            Area = area;
            FloorId = floorId;
        }

        public void Handle(IMap map) => Handle(map.Id);

        public void Handle(MapId mapId)
        {
            Handled = true;
            ResultMapId = mapId;
        }
    }
}

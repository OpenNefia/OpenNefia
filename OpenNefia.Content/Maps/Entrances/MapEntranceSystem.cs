using OpenNefia.Content.Levels;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Maps
{
    public interface IMapEntranceSystem : IEntitySystem
    {
        bool TryGetAreaOfEntrance(MapEntrance entrance, [NotNullWhen(true)] out IArea? area);
        bool UseMapEntrance(EntityUid user, MapEntrance entrance, bool silent = false, SpatialComponent? spatial = null);
        bool UseMapEntrance(EntityUid user, MapEntrance entrance, [NotNullWhen(true)] out MapId? mapId, bool silent = false, SpatialComponent? spatial = null);

        /// <summary>
        /// Sets the map to travel to when exiting the destination map via the edges.
        /// </summary>
        /// <param name="targetMap">Map that is being travelled to.</param>
        /// <param name="prevCoords">Location that exiting the given map from the edges should lead to.</param>
        void SetPreviousMap(IMap targetMap, MapCoordinates prevCoords);
        void SetPreviousMap(MapId targetMap, MapCoordinates prevCoords);

        int GetFloorNumber(IMap map);
    }

    public sealed class MapEntranceSystem : EntitySystem, IMapEntranceSystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IGameSessionManager _session = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public bool TryGetAreaOfEntrance(MapEntrance entrance, [NotNullWhen(true)] out IArea? area)
        {
            var entranceAreaId = entrance.MapIdSpecifier.GetOrGenerateAreaId();
            if (entranceAreaId == null)
            {
                area = null;
                return false;
            }

            return _areaManager.TryGetArea(entranceAreaId.Value, out area);
        }

        public bool UseMapEntrance(EntityUid user, MapEntrance entrance, bool silent = false,
            SpatialComponent? spatial = null)
            => UseMapEntrance(user, entrance, out _, silent, spatial);

        public bool UseMapEntrance(EntityUid user, MapEntrance entrance,
            [NotNullWhen(true)] out MapId? mapId, bool silent = false,
            SpatialComponent? spatial = null)
        {
            mapId = null;

            if (!Resolve(user, ref spatial))
                return false;

            var ev = new BeforeUseMapEntranceEvent(entrance);
            RaiseEvent(user, ev);
            if (ev.Cancelled || !IsAlive(user))
                return false;

            if (!silent)
                _audio.Play(Protos.Sound.Exitmap1);

            mapId = entrance.MapIdSpecifier.GetOrGenerateMapId();
            if (mapId == null)
            {
                Logger.WarningS("map.entrance", $"Failed to get map ID for entrance {entrance}!");
                return false;
            }

            if (!_mapLoader.TryGetOrLoadMap(mapId.Value, _saveGameManager.CurrentSave!, out var map))
                return false;

            var newPos = entrance.StartLocation.GetStartPosition(user, map)
                .BoundWithin(map.Bounds);

            _mapTransfer.DoMapTransfer(spatial, map, map.AtPosEntity(newPos), MapLoadType.Traveled);

            return true;
        }

        /// <inheritdoc/>
        public void SetPreviousMap(MapId targetMap, MapCoordinates prevCoords)
            => SetPreviousMap(_mapManager.GetMap(targetMap), prevCoords);

        /// <inheritdoc/>
        public void SetPreviousMap(IMap targetMap, MapCoordinates prevCoords)
        {
            var mapEntityUid = targetMap.MapEntityUid;
            var mapMapEntrance = EntityManager.EnsureComponent<MapEdgesEntranceComponent>(mapEntityUid);
            mapMapEntrance.Entrance = MapEntrance.FromMapCoordinates(prevCoords);
        }

        public int GetFloorNumber(IMap map)
        {
            var common = EnsureComp<MapCommonComponent>(map.MapEntityUid);
            if (common.FloorNumber != null)
                return common.FloorNumber.Value;

            if (TryComp<LevelComponent>(map.MapEntityUid, out var level))
                return level.Level;

            return 0;
        }
    }

    public sealed class BeforeUseMapEntranceEvent : CancellableEntityEventArgs
    {
        public MapEntrance Entrance { get; }

        public BeforeUseMapEntranceEvent(MapEntrance entrance)
        {
            Entrance = entrance;
        }
    }
}

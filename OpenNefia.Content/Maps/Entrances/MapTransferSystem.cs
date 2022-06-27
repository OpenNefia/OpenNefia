using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Game;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Parties;
using OpenNefia.Core.Areas;

namespace OpenNefia.Content.Maps
{
    public interface IMapTransferSystem : IEntitySystem
    {
        void DoMapTransfer(SpatialComponent playerSpatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType);
    }

    public partial class MapTransferSystem : EntitySystem, IMapTransferSystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        
        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, ExitingMapFromEdgesEventArgs>(HandleExitMapFromEdges, nameof(HandleExitMapFromEdges));
            SubscribeLocalEvent<MapComponent, ActiveMapChangedEvent>(HandleActiveMapChanged, nameof(HandleActiveMapChanged));
            SubscribeLocalEvent<MapComponent, MapLeaveEventArgs>(HandleLeaveMap, nameof(HandleLeaveMap));
        }

        public void DoMapTransfer(SpatialComponent spatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType)
        {
            if (newCoords.GetMapId(EntityManager) != map.Id)
                throw new ArgumentException($"Coordinates {newCoords} are not within map ${map}!");

            if (newCoords.GetMapId(EntityManager) == _mapManager.ActiveMap?.Id)
            {
                spatial.Coordinates = newCoords;
                return;
            }

            if (_gameSession.IsPlayer(spatial.Owner))
            {
                TransferPlayer(spatial, map, newCoords, loadType);
            }
            else
            {
                spatial.Coordinates = newCoords;
            }
        }

        public void TransferPlayer(SpatialComponent playerSpatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType)
        {
            var oldMap = _mapManager.ActiveMap;

            TransferPlayerParty(playerSpatial, newCoords);

            if (oldMap != null)
            {
                var evLeave = new MapLeaveEventArgs(map, oldMap);
                RaiseLocalEvent(oldMap.MapEntityUid, evLeave);
            }

            _mapManager.SetActiveMap(map.Id, loadType);

            // Unload the old map.
            if (oldMap != null)
            {
                var save = _saveGameManager.CurrentSave!;

                var mapCommon = EntityManager.GetComponent<MapCommonComponent>(oldMap.MapEntityUid);
                var isTemporary = mapCommon.IsTemporary;

                if (!isTemporary)
                {
                    _mapLoader.SaveMap(oldMap.Id, save);
                }

                _mapManager.UnloadMap(oldMap.Id);

                if (isTemporary)
                {
                    Logger.WarningS("map.transfer", $"Deleting temporary map {oldMap.Id}.");
                    _mapLoader.DeleteMap(oldMap.Id, save);
                }
            }
        }

        private void TransferPlayerParty(SpatialComponent playerSpatial, EntityCoordinates newCoords)
        {
            var mapCoords = newCoords.ToMap(EntityManager);

            var player = playerSpatial.Owner;
            DebugTools.Assert(_placement.TryPlaceChara(player, mapCoords), $"Could not place player in {mapCoords}/{newCoords}!");

            foreach (var ally in _parties.EnumerateUnderlings(_gameSession.Player))
            {
                _placement.TryPlaceChara(ally, mapCoords);
            }
        }
    }

    public sealed class MapLeaveEventArgs : EntityEventArgs
    {
        public IMap NewMap { get; }
        public IMap OldMap { get; }

        public MapLeaveEventArgs(IMap newMap, IMap oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }
}

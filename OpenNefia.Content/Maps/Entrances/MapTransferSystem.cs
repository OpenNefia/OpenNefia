using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Game;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Parties;
using OpenNefia.Core.Areas;
using OpenNefia.Content.EntityGen;
using static NetVips.Enums;
using ICSharpCode.Decompiler.IL;

namespace OpenNefia.Content.Maps
{
    public interface IMapTransferSystem : IEntitySystem
    {
        void DoMapTransfer(SpatialComponent playerSpatial, IMap map, IMapStartLocation location, MapLoadType loadType = MapLoadType.Full, bool noUnloadPrevious = false);
        void DoMapTransfer(SpatialComponent playerSpatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType = MapLoadType.Full, bool noUnloadPrevious = false);
        void RunMapInitializeEvents(IMap map, MapLoadType loadType);
    }

    public partial class MapTransferSystem : EntitySystem, IMapTransferSystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IMapPlacement _placement = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ITemporaryEntitySystem _tempEntities = default!;

        public void DoMapTransfer(SpatialComponent spatial, IMap map, IMapStartLocation location, MapLoadType loadType = MapLoadType.Full, bool noUnloadPrevious = false)
            => DoMapTransfer(spatial, map, map.AtPosEntity(location.GetStartPosition(spatial.Owner, map)), loadType, noUnloadPrevious);

        public void DoMapTransfer(SpatialComponent spatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType = MapLoadType.Full, bool noUnloadPrevious = false)
        {
            var newMapId = newCoords.GetMapId(EntityManager);
            if (newMapId != map.Id)
                throw new ArgumentException($"Coordinates {newCoords} (MapId: {newMapId}) are not within map ${map}!");

            if (newMapId == _mapManager.ActiveMap?.Id)
            {
                spatial.Coordinates = newCoords;
                return;
            }

            if (_gameSession.IsPlayer(spatial.Owner))
            {
                TransferPlayer(spatial, map, newCoords, loadType, noUnloadPrevious);
            }
            else
            {
                spatial.Coordinates = newCoords;
            }
        }

        public void TransferPlayer(SpatialComponent playerSpatial, IMap map, EntityCoordinates newCoords, MapLoadType loadType, bool noUnloadPrevious = false)
        {
            var oldMap = _mapManager.ActiveMap;

            if (oldMap != null)
            {
                var evLeave = new BeforeMapLeaveEventArgs(map, oldMap);
                RaiseEvent(oldMap.MapEntityUid, evLeave);

                var evLeave2 = new BeforePartyMemberLeavesMapEventArgs(map, oldMap);
                foreach (var chara in _parties.EnumerateMembers(playerSpatial.Owner))
                    RaiseEvent(chara, evLeave2);
            }

            TransferPlayerParty(playerSpatial, newCoords, oldMap?.Id);

            _mapManager.SetActiveMap(map.Id, loadType);

            var evEnter = new AfterMapEnterEventArgs(map, oldMap);
            RaiseEvent(map.MapEntityUid, evEnter);

            var evEnter2 = new AfterPartyMemberEntersMapEventArgs(map, oldMap);
            foreach (var chara in _parties.EnumerateMembers(playerSpatial.Owner))
                RaiseEvent(chara, evEnter2);

            // Unload the old map.
            if (oldMap != null && !noUnloadPrevious)
            {
                var save = _saveGameManager.CurrentSave!;

                var mapCommon = EntityManager.GetComponent<MapCommonComponent>(oldMap.MapEntityUid);
                var isTemporary = mapCommon.IsTemporary;
                var unloadType = isTemporary ? MapUnloadType.Delete : MapUnloadType.Unload;

                if (!isTemporary)
                {
                    _mapLoader.SaveMap(oldMap.Id, save);
                }

                _mapManager.UnloadMap(oldMap.Id, unloadType);

                if (isTemporary)
                {
                    Logger.WarningS("map.transfer", $"Deleting temporary map {oldMap.Id}.");
                    _mapLoader.DeleteMap(oldMap.Id, save);
                }
            }

            _tempEntities.ClearGlobalTemporaryEntities();
            _turnOrder.AdvanceState();

            GC.Collect();
        }

        private void TransferPlayerParty(SpatialComponent playerSpatial, EntityCoordinates newCoords, MapId? oldMapID)
        {
            var mapCoords = newCoords.ToMap(EntityManager);

            var player = playerSpatial.Owner;
            DebugTools.Assert(_placement.TryPlaceChara(player, mapCoords), $"Could not place player in {mapCoords}/{newCoords}!");

            foreach (var ally in _parties.EnumerateUnderlings(_gameSession.Player))
            {
                // Account for allies that might have been moved out of the old map already by BeforeMapLeaveEvent,
                // such as through StayersSystem
                if (oldMapID != null && Spatial(ally).MapID == oldMapID)
                    _placement.TryPlaceChara(ally, mapCoords);
            }
        }
    }

    /// <summary>
    /// Raised before a map is left. Event is raised on the *old map*. Player and party
    /// will still be located in the old map.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public sealed class BeforeMapLeaveEventArgs : EntityEventArgs
    {
        /// <summary>
        /// Map being entered.
        /// </summary>
        public IMap NewMap { get; }

        /// <summary>
        /// Map being left. The player and party will be located in this map.
        /// </summary>
        public IMap OldMap { get; }

        public BeforeMapLeaveEventArgs(IMap newMap, IMap oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class BeforePartyMemberLeavesMapEventArgs : EntityEventArgs
    {
        /// <summary>
        /// Map being entered.
        /// </summary>
        public IMap NewMap { get; }

        /// <summary>
        /// Map being left. The player and party will be located in this map.
        /// </summary>
        public IMap OldMap { get; }

        public BeforePartyMemberLeavesMapEventArgs(IMap newMap, IMap oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }

    /// <summary>
    /// Raised after a map is entered. Event is raised on the *new map*s. Player and party
    /// will have been moved to the new map.
    /// </summary>
    [EventUsage(EventTarget.Map)]
    public sealed class AfterMapEnterEventArgs : EntityEventArgs
    {
        /// <summary>
        /// Map that was entered. The player and party will be located in this map.
        /// </summary>
        public IMap NewMap { get; }

        /// <summary>
        /// Map that was left. This can be <c>null</c> if a save is being loaded.
        /// </summary>
        public IMap? OldMap { get; }

        public AfterMapEnterEventArgs(IMap newMap, IMap? oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class AfterPartyMemberEntersMapEventArgs : EntityEventArgs
    {
        /// <summary>
        /// Map that was entered. The player and party will be located in this map.
        /// </summary>
        public IMap NewMap { get; }

        /// <summary>
        /// Map that was left. This can be <c>null</c> if a save is being loaded.
        /// </summary>
        public IMap? OldMap { get; }

        public AfterPartyMemberEntersMapEventArgs(IMap newMap, IMap? oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }
}

using OpenNefia.Content.Effects.New.Unique;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Game;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Timers;
using OpenNefia.Content.World;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.Areas;
using PrettyPrompt;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.Home;
using OpenNefia.Core.Log;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.DisplayName;
using System.Xml.Linq;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Quests;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.Return
{
    /// <summary>
    /// Shared behavior for the Return and Escape spells
    /// that transports the player to another location
    /// after a set time period.
    /// </summary>
    public interface IReturnSystem : IEntitySystem
    {
        bool IsReturnInProgress => ReturnState != null;
        ReturnState? ReturnState { get; }

        /// <summary>
        /// Checks if the player can return out of the current map.
        /// Queries for confirmation if necessary.
        /// </summary>
        /// <returns>True if return should proceed.</returns>
        bool CheckReturnCapability(out string? reason);

        void CancelReturn();

        void StartReturn(MapEntrance location, int turnsUntilReturn);
        bool TryPromptReturnLocation(IMap sourceMap, [NotNullWhen(true)] out MapEntrance? entrance);

        bool TryGetEscapeLocation(IMap innerMap, [NotNullWhen(true)] out MapCoordinates? outerAreaEntranceCoords);
    }

    [DataDefinition]
    public sealed class ReturnState
    {
        public ReturnState() { }

        public ReturnState(string globalTimerID, MapEntrance targetLocation)
        {
            GlobalTimerID = globalTimerID;
            TargetLocation = targetLocation;
        }

        [DataField]
        public string GlobalTimerID { get; } = string.Empty;

        [DataField]
        public MapEntrance TargetLocation { get; } = new();
    }

    public sealed class ReturnSystem : EntitySystem, IReturnSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IPlayerQuery _playerQueries = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGlobalTimersSystem _globalTimers = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IAreaKnownEntrancesSystem _areaKnownEntrances = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IAreaEntranceSystem _areaEntrances = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly ICargoSystem _cargos = default!;
        [Dependency] private readonly IMapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IQuestSystem _quests = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        [RegisterSaveData("Elona.ReturnSystem.ReturnState")]
        public ReturnState? ReturnState { get; private set; } = null;

        public const string TimerID = "Elona.ReturnSystem.ReturnTimer";

        public override void Initialize()
        {
            SubscribeEntity<AfterMapEnterEventArgs>(VisitArea);
            SubscribeBroadcast<MapBeforeTurnBeginEventArgs>(InvokeReturn, priority: EventPriorities.VeryLow);
            SubscribeComponent<PartyComponent, BeforeReturnMagicExecutedEvent>(PreventIfTempAlliesExist);
            SubscribeComponent<InventoryComponent, BeforeReturnMagicExecutedEvent>(PreventIfOverburdened);
            SubscribeComponent<CargoHolderComponent, BeforeReturnMagicExecutedEvent>(PreventIfCargoOverburdened);
        }

        private void VisitArea(EntityUid uid, AfterMapEnterEventArgs args)
        {
            if (!TryArea(args.NewMap, out var area))
                return;

            var areaReturnDest = EnsureComp<AreaReturnDestinationComponent>(area.AreaEntityUid);
            areaReturnDest.HasEverBeenVisited = true;

            if (_areaManager.TryGetAreaAndFloorOfMap(args.NewMap.Id, out _, out var floorId))
                areaReturnDest.DeepestFloorVisited = int.Max(floorId.Value.FloorNumber, areaReturnDest.DeepestFloorVisited);
        }

        public bool CheckReturnCapability(out string? reason)
        {
            var returner = _gameSession.Player;
            reason = null;
            if (!TryMap(returner, out var map))
                return false;

            var ev = new BeforePlayerCastsReturnMagicEvent(map);
            RaiseEvent(returner, ev);

            var result = PreventReturnSeverity.Proceed;
            var messages = new List<string>();
            foreach (var warning in ev.OutWarningReasons)
            {
                messages.Append(warning.Message + Loc.Space);
                result = EnumHelpers.Max(result, warning.Severity);
            }

            var messageStr = string.Join(Loc.Space, messages);

            if (result == PreventReturnSeverity.PromptYesNo)
            {
                if (!_playerQueries.YesOrNo(messageStr))
                    return false;
            }
            else if (result == PreventReturnSeverity.Fail)
            {
                reason = messageStr;
                return false;
            }

            return true;
        }

        public record class ReturnLocation(MapId MapID, string Name) : IPromptFormattable
        {
            public string FormatForPrompt() => Name;
        }

        private bool CanReturnTo(IArea area)
        {
            return TryComp<AreaReturnDestinationComponent>(area.AreaEntityUid, out var retDest)
                && retDest.CanBeReturnDestination
                && retDest.HasEverBeenVisited;
        }

        // TODO i don't know about this, instead of placing the return location on
        // the area (since map and area relationships can change) this just replicates
        // vanilla's behavior by inspecting properties of the area. i think the latter approach
        // *is* the right idea, but i'm not not totally satisfied with it yet.
        private ReturnLocation? AreaToReturnLocation(IArea area)
        {
            var comp = Comp<AreaReturnDestinationComponent>(area.AreaEntityUid);

            if (comp.ReturnFloor != null)
            {
                if (area.ContainedMaps.TryGetValue(comp.ReturnFloor.Value, out var returnFloor) && returnFloor.MapId != null)
                {
                    var name = _displayNames.GetDisplayName(area.AreaEntityUid);
                    var floorNum = comp.ReturnFloor.Value.FloorNumber;
                    if (floorNum != AreaFloorId.DefaultFloorNumber)
                        name += Loc.Space + Loc.GetString("Elona.Return.LevelCounter", ("level", floorNum));
                    return new ReturnLocation(returnFloor.MapId.Value, name);
                }
                Logger.ErrorS("return", $"Area {area} has invalid return preset floor {comp.ReturnFloor.Value}!");
            }

            // For dungeons and non-towns, the player should return to the deepest floor they've visited
            // NOTE: this assumes the default floor name is used!
            var floorID = new AreaFloorId(AreaFloorId.DefaultFloorName, comp.DeepestFloorVisited);

            if (HasComp<AreaTypeTownComponent>(area.AreaEntityUid))
            {
                // The player should return to the starting floor if it's a town
                // Otherwise the floor closest to the surface
                if (TryComp<AreaEntranceComponent>(area.AreaEntityUid, out var areaEnt))
                    floorID = areaEnt.StartingFloor;
                else
                    floorID = area.ContainedMaps.Keys.OrderBy(f => f.FloorNumber).FirstOrNull() ?? AreaFloorId.Default;
                Logger.DebugS("return", $"Return floor for town area {area}: {floorID}");
            }
            else
            {
                Logger.DebugS("return", $"Return floor for non-town area {area}: {floorID}");
            }

            if (area.ContainedMaps.TryGetValue(floorID, out var floor) && floor.MapId != null)
            {
                var name = _displayNames.GetDisplayName(area.AreaEntityUid);
                var floorNum = floorID.FloorNumber;
                if (floorNum != AreaFloorId.DefaultFloorNumber)
                    name += Loc.Space + Loc.GetString("Elona.Return.LevelCounter", ("level", floorNum));
                return new ReturnLocation(floor.MapId.Value, _displayNames.GetDisplayName(area.AreaEntityUid));
            }

            return null;
        }

        public bool TryPromptReturnLocation(IMap sourceMap, [NotNullWhen(true)] out MapEntrance? entrance)
        {
            // >>>>>>>> elona122/shade2/command.hsp:4388 	p=0:i=areaHome ...
            var candidates = new List<ReturnLocation>();

            // TODO doesn't check for containment of home in world map (yet)!
            // in Elona+ you're only able to return to homes/locations within
            // the current "global" area.
            foreach (var home in _homes.ActiveHomeIDs)
            {
                var name = "????";
                if (TryArea(home, out var homeArea))
                    name = _displayNames.GetDisplayName(homeArea.AreaEntityUid);
                candidates.Add(new(home, name));
            }

            // The player is intended to be contained inside a "global" area
            // like North Tyris. Which area is the global area is updated each
            // time the player changes maps. Check the Return locations inside
            // child areas of this area.
            if (_areaEntrances.TryGetCurrentGlobalArea(out var globalArea))
            {
                var otherCandidates = _areaManager.EnumerateChildAreas(globalArea)
                    .Where(CanReturnTo)
                    .Select(AreaToReturnLocation)
                    .WhereNotNull()
                    .ToList();

                candidates.AddRange(otherCandidates);
            }
            else
            {
                Logger.WarningS("return", $"No current global area while inside map {sourceMap}.");
            }

            if (candidates.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Return.NoLocations"));
                entrance = null;
                return false;
            }

            var opts = new Prompt<ReturnLocation>.Args()
            {
                IsCancellable = true,
                QueryText = Loc.GetString("Elona.Return.Prompt"),
            };
            var result = _playerQueries.PickOrNone(candidates, opts);
            if (result == null)
            {
                entrance = null;
                return false;
            }

            entrance = new MapEntrance(new BasicMapIdSpecifier(result.MapID), new MapOrAreaEntityStartLocation());
            return true;
            // <<<<<<<< elona122/shade2/command.hsp:4414 	gosub *screen_draw ...
        }

        public void CancelReturn()
        {
            _globalTimers.RemoveTimer(TimerID, raiseEvent: false);
            ReturnState = null;
        }

        public void StartReturn(MapEntrance location, int turnsUntilReturn)
        {
            var turns = int.Max(turnsUntilReturn, 1);
            _globalTimers.AddOrUpdateTimer(TimerID, GameTimeSpan.FromMinutes(turns), TimerUpdateType.MapTurnsPassed);
            ReturnState = new(TimerID, location);
        }

        private void InvokeReturn(MapBeforeTurnBeginEventArgs ev)
        {
            // >>>>>>>> elona122/shade2/main.hsp:737 			if (mType=mTypeQuest)or(gArea=areaShelter)or(gA ...
            if (ReturnState == null)
                return;

            if (_deferredEvents.IsEventEnqueued())
                return;


            // >>>>>>>> elona122/shade2/main.hsp:741 			if (gReturn<=0)and(evNum=0){ ...
            if (!_globalTimers.TryGetTimer(TimerID, out var timer) || timer.TimeRemaining <= GameTimeSpan.Zero)
            {
                var entrance = ReturnState.TargetLocation;
                ReturnState = null;

                var ev2 = new BeforeReturnMagicExecutedEvent(entrance);
                RaiseEvent(_gameSession.Player, ev2);
                if (ev2.Cancelled)
                    return;

                _audio.Play(Protos.Sound.Teleport1, _gameSession.Player);
                _mes.Display(Loc.GetString("Elona.Return.Result.DimensionalDoorOpens"));

                var mapID = entrance.MapIdSpecifier.GetMapId();
                if (mapID == _mapManager.ActiveMap?.Id)
                {
                    _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                    return;
                }

                // TODO jail message

                _playerQueries.PromptMore();
                _mapEntrances.UseMapEntrance(_gameSession.Player, entrance, silent: true);
            }
            // <<<<<<<< elona122/shade2/main.hsp:757 				} ...
        }

        private void PreventIfTempAlliesExist(EntityUid uid, PartyComponent component, BeforeReturnMagicExecutedEvent args)
        {
            if (args.Cancelled)
                return;

            // >>>>>>>> elona122/shade2/main.hsp:742 				f=false ...
            bool IsTempAlly(EntityUid ent)
            {
                return (TryComp<TemporaryAllyComponent>(ent, out var tempAlly) && !tempAlly.AllowsReturning) 
                    || HasComp<EscortedInQuestComponent>(ent);
            }

            if (_parties.EnumerateMembers(uid, component).Any(IsTempAlly))
            {
                _mes.Display(Loc.GetString("Elona.Return.Result.AllyPrevents"));
                args.Cancel();
            }
            // <<<<<<<< elona122/shade2/main.hsp:747 				if f: txt lang("今は帰還できない仲間を連れている。","One of you ...
        }

        private void PreventIfOverburdened(EntityUid uid, InventoryComponent component, BeforeReturnMagicExecutedEvent args)
        {
            if (args.Cancelled)
                return;

            // >>>>>>>> elona122/shade2/main.hsp:748 				if (dbg_noWeight=0)&(cBurden(pc)>=burdenMax){ ...
            if (component.BurdenType >= BurdenType.Max)
            {
                _mes.Display(Loc.GetString("Elona.Return.Result.Overburdened"));
                args.Cancel();
            }

            // XXX: allies?

            // <<<<<<<< elona122/shade2/main.hsp:750 					} ...
        }

        /// <summary>
        /// Ported from Elona+.
        /// </summary>
        private void PreventIfCargoOverburdened(EntityUid uid, CargoHolderComponent component, BeforeReturnMagicExecutedEvent args)
        {
            if (args.Cancelled)
                return;

            if (_cargos.IsBurdenedByCargo(uid))
            {
                _mes.Display(Loc.GetString("Elona.Return.Result.CargoOverburdened"));
                args.Cancel();
            }

            // XXX: allies?
        }

        public bool TryGetEscapeLocation(IMap innerMap, [NotNullWhen(true)] out MapCoordinates? coords)
        {
            if (!TryArea(innerMap, out var area))
            {
                coords = null;
                return false;
            }

            if (!_areaKnownEntrances.TryGetEntranceTo(area, out var entrance))
            {
                coords = null;
                return false;
            }

            coords = entrance.MapCoordinates;
            return true;
        }
    }

    /// <summary>
    /// Raised when the Return spell timer expires after the delay, just before
    /// the player is transported to another map.
    /// </summary>
    [EventUsage(EventTarget.Normal)]
    public sealed class BeforeReturnMagicExecutedEvent : CancellableEntityEventArgs
    {
        public BeforeReturnMagicExecutedEvent(MapEntrance entrance)
        {
            Entrance = entrance;
        }

        public MapEntrance Entrance { get; }
    }
}
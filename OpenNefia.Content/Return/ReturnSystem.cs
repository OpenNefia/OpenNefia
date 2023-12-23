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

        [RegisterSaveData("Elona.ReturnSystem.ReturnState")]
        public ReturnState? ReturnState { get; private set; } = null;

        public const string TimerID = "Elona.ReturnSystem.ReturnTimer";

        public override void Initialize()
        {
            SubscribeEntity<AfterMapEnterEventArgs>(VisitArea);
            SubscribeBroadcast<MapBeforeTurnBeginEventArgs>(InvokeReturn, priority: EventPriorities.VeryLow);
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

        public record class ReturnLocation(MapId mapID);

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
                    return new ReturnLocation(returnFloor.MapId.Value);
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
                return new ReturnLocation(floor.MapId.Value);

            return null;
        }

        public bool TryPromptReturnLocation(IMap sourceMap, [NotNullWhen(true)] out MapEntrance? entrance)
        {
            // >>>>>>>> elona122/shade2/command.hsp:4388 	p=0:i=areaHome ...
            if (!TryArea(sourceMap, out var area))
            {
                entrance = null;
                return false;
            }

            var candidates = new List<ReturnLocation>();

            // TODO doesn't check for containment of home in world map (yet)!
            // in Elona+ you're only able to return to homes/locations within
            // the current "global" area.
            foreach (var home in _homes.ActiveHomeIDs)
            {
                candidates.Add(new(home));
            }

            var root = _areaManager.GetRootArea(area);
            var otherCandidates = _areaManager.EnumerateChildAreas(root)
                .Where(CanReturnTo)
                .Select(AreaToReturnLocation)
                .WhereNotNull()
                .ToList();

            candidates.AddRange(otherCandidates);

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

            entrance = new MapEntrance(new BasicMapIdSpecifier(result.mapID), new MapOrAreaEntityStartLocation());
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

            if (!_globalTimers.TryGetTimer(TimerID, out var timer) || timer.TimeRemaining <= GameTimeSpan.Zero)
            {

            }
            // <<<<<<<< elona122/shade2/main.hsp:757 				} ...
        }

        public bool TryGetEscapeLocation(IMap innerMap, [NotNullWhen(true)] out MapCoordinates? outerAreaEntranceCoords)
        {

        }
    }
}
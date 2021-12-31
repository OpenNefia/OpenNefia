using OpenNefia.Analyzers;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.TurnOrder
{
    public interface ITurnOrderSystem
    {
        /// <summary>
        /// Returns true if the simulation is active (map is being displayed, etc.)
        /// </summary>
        bool IsInGame();

        /// <summary>
        /// Calculates the raw speed value for an entity.
        /// </summary>
        int CalculateSpeed(EntityUid entity, TurnOrderComponent? turnOrder = null);

        /// <summary>
        /// Resets the state and starts a new turn.
        /// </summary>
        void InitializeState();

        /// <summary>
        /// Advances the state of the simulation until additional input or exiting are needed.
        /// </summary>
        void AdvanceStateFromPlayer(TurnResult turnResult);
    }

    /// <summary>
    /// Driver for turn order logic. Handles passing player/NPC turns via a finite state machine.
    /// </summary>
    /// <remarks>
    /// <para>
    /// How turn ordering works:
    /// </para>
    /// <para>
    /// When a map is first entered, the list of entities to run turn actions on is calculated. 
    /// In HSP Elona, this simply iterates the entire list of characters from 0 to the maximum index. Since
    /// index 0 indicates the player, and 1-16 indicate allies, this means the player and allies are checked
    /// first for the ability to take an action, followed by other characters.
    /// </para>
    /// <para>
    /// Then, the initial "turn budget" each character gets is calculated. More budget means more actions.
    /// When a turn begins, each character's turn budget is replenished based on their speed. Each turn, every
    /// character in the map is iterated. if a character has a greater budget than the cost per action for the map,
    /// they can take an action and subtract the cost from their budget. Otherwise, their turn is skipped until their 
    /// turn budget is replenished from waiting enough consecutive turns to make a move.
    /// </para>
    /// </remarks>
    public class TurnOrderSystem : EntitySystem, ITurnOrderSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        private TurnOrderState _state = TurnOrderState.TurnBegin;

        private List<TurnOrderComponent> _entityOrder = new();
        private IEnumerator<TurnOrderComponent>? _currentTurnOrder;
        private TurnOrderComponent? _activeEntity;

        public override void Initialize()
        {
            _mapManager.ActiveMapChanged += OnActiveMapChanged;

            SubscribeLocalEvent<MapTurnOrderComponent, MapChangedEventArgs>(HandleMapChangedTurnOrder, nameof(HandleMapChangedTurnOrder));
        }

        #region Event Handlers

        private void OnActiveMapChanged(IMap map, IMap? oldMap)
        {
            InitializeState();

            var ev = new MapChangedEventArgs(map, oldMap);
            RaiseLocalEvent(map.MapEntityUid, ev);
        }

        private void HandleMapChangedTurnOrder(EntityUid uid, MapTurnOrderComponent mapTurnOrder, MapChangedEventArgs args)
        {
            mapTurnOrder.IsFirstTurn = true;

            InitializeMap(args.NewMap);
        }

        private void InitializeMap(IMap map)
        {
            foreach (var turnOrder in _lookup.EntityQueryInMap<TurnOrderComponent>(map.Id))
            {
                turnOrder.TimeThisTurn = 0;
            }
        }

        #endregion

        #region ITurnOrder Implementation

        /// <inheritdoc/>
        public bool IsInGame()
        {
            return _gameSession.Player != null && _field.IsInActiveLayerList() && _mapManager.ActiveMap != null;
        }

        /// <inheritdoc/>
        public int CalculateSpeed(EntityUid entity, TurnOrderComponent? turnOrder = null)
        {
            if (!Resolve(entity, ref turnOrder))
            {
                return 10;
            }

            return (int)MathF.Max(turnOrder.CurrentSpeed * turnOrder.SpeedPercentage, 10f);
        }
        
        /// <inheritdoc/>
        public void InitializeState()
        {
            Logger.InfoS("turn", $"Starting new turn order state for map {_mapManager.ActiveMap?.Id}");

            _state = TurnOrderState.TurnBegin;
            _entityOrder.Clear();
            _currentTurnOrder = null;
            _activeEntity = null;

            DoAdvanceState();
        }

        /// <inheritdoc/>
        public void AdvanceStateFromPlayer(TurnResult turnResult)
        {
            _state = turnResult.ToTurnOrderState(isPlayerTurn: true);
            DoAdvanceState();
        }

        private void DoAdvanceState()
        {
            while (true)
            {
                var old = _state;
                _state = RunStateChange(_state);

                switch (_state)
                {
                    case TurnOrderState.TitleScreen:
                        QuitToTitleScreen();
                        return;
                    case TurnOrderState.PlayerTurnQuery:
                        return;
                }
            }
        }

        private void QuitToTitleScreen()
        {
            if (_field.IsQuerying())
            {
                _field.Cancel();
            }
        }

        #endregion

        #region State Transitions

        /// <summary>
        /// Injects a turn order state transition into the simulation.
        /// </summary>
        /// <param name="state">The new state.</param>
        /// <returns>The new state.</returns>
        internal TurnOrderState RunStateChange(TurnOrderState state)
        {
            if (!IsInGame())
            {
                return TurnOrderState.PlayerTurnQuery;
            }

            return state switch
            {
                TurnOrderState.TurnBegin => DoTurnBegin(),
                TurnOrderState.TurnEnd => DoTurnEnd(),
                TurnOrderState.PlayerDied => DoPlayerDied(),
                TurnOrderState.PassTurns => DoPassTurns(),
                TurnOrderState.PlayerTurnQuery => TurnOrderState.PlayerTurnQuery,
                TurnOrderState.TitleScreen => TurnOrderState.PlayerTurnQuery,
                _ => TurnOrderState.PlayerTurnQuery
            };
        }

        private List<TurnOrderComponent> CalculateTurnOrder(IMap map)
        {
            // No logic is run for maps besides the one the player is in.
            if (_gameSession.Player?.Spatial.MapID != map.Id)
                return new();

            // Order the player to always go first.
            // TODO: Order allies to go after the player.
            int TurnOrderComparer(TurnOrderComponent arg)
            {
                if (_gameSession.IsPlayer(arg.OwnerUid))
                    return -1;

                return (int)arg.OwnerUid;
            }

            return _lookup.EntityQueryInMap<TurnOrderComponent>(map.Id, includeChildren: false)
                .Where(ent => EntityManager.IsAlive(ent.OwnerUid))
                .OrderBy(TurnOrderComparer)
                .ToList();
        }

        /// <summary>
        /// Replenishes the time budget for all turn orderable entities in the map.
        /// </summary>
        /// <remarks>
        /// Every entity gets a time budget proportional to the player's time budget.
        /// </remarks>
        private void UpdateTimeThisTurn(IEnumerable<TurnOrderComponent> entityOrder, int playerTimeThisTurn)
        {
            foreach (var turnOrder in entityOrder)
            {
                turnOrder.TimeThisTurn += CalculateSpeed(turnOrder.OwnerUid, turnOrder) * playerTimeThisTurn;
            }
        }

        private TimeSpan TimeUnitsToSeconds(int timeUnits)
        {
            return TimeSpan.FromSeconds(timeUnits / 5 + 1);
        }

        private TurnOrderState DoTurnBegin()
        {
            _activeEntity = null;

            var map = _mapManager.ActiveMap!;
            var player = _gameSession.Player.Uid;

            var ev = new BeforeTurnBeginEventArgs();
            if (Raise(map.MapEntityUid, ev))
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            if (!EntityManager.IsAlive(player))
            {
                return TurnOrderState.PlayerDied;
            }

            _entityOrder = CalculateTurnOrder(map);

            var playerTurnOrder = EntityManager.EnsureComponent<TurnOrderComponent>(player);
            var playerSpeed = CalculateSpeed(player, playerTurnOrder);
            var mapTurnOrder = EntityManager.EnsureComponent<MapTurnOrderComponent>(map.MapEntityUid);

            // All characters will start with at least this much time during
            // this turn.
            var startingTurnTime = (mapTurnOrder.TurnCost - playerTurnOrder.TimeThisTurn) / playerSpeed + 1;

            UpdateTimeThisTurn(_entityOrder, startingTurnTime);

            if (!mapTurnOrder.IsFirstTurn)
            {
                _world.PassTime(TimeUnitsToSeconds(startingTurnTime / 5 + 1));
            }

            return TurnOrderState.PassTurns;
        }

        private TurnOrderComponent? GetNextEntityInOrder(MapTurnOrderComponent mapTurnOrder)
        {
            if (_currentTurnOrder == null || _currentTurnOrder.Current == null)
            {
                _currentTurnOrder = _entityOrder.GetEnumerator();
                _currentTurnOrder.MoveNext();
            }

            TurnOrderComponent? found = null;
            TurnOrderComponent? current = _currentTurnOrder.Current;
            var didFullSearch = false;

            while (true)
            {
                while (found == null && current != null)
                {
                    if (EntityManager.IsAlive(current.OwnerUid))
                    {
                        if (current.TimeThisTurn >= mapTurnOrder.TurnCost)
                        {
                            found = current;
                            _currentTurnOrder.MoveNext();
                            break;
                        }
                    }

                    if (!_currentTurnOrder.MoveNext())
                        break;

                    current = _currentTurnOrder.Current;
                }

                if (didFullSearch)
                {
                    break;
                }

                didFullSearch = true;
                _currentTurnOrder = _entityOrder.GetEnumerator();
            }

            return found;
        }

        private TurnOrderState DoPassTurns()
        {
            var map = _mapManager.ActiveMap!;
            var mapTurnComp = EntityManager.EnsureComponent<MapTurnOrderComponent>(map.MapEntityUid);
            var nextInOrder = GetNextEntityInOrder(mapTurnComp);

            if (nextInOrder == null)
            {
                return TurnOrderState.TurnBegin;
            }

            var isFirstTurn = mapTurnComp.IsFirstTurn;
            mapTurnComp.IsFirstTurn = false;

            nextInOrder.TimeThisTurn -= mapTurnComp.TurnCost;
            nextInOrder.TurnsAlive++;

            _activeEntity = nextInOrder;

            var ev = new EntityTurnStartingEventArgs(isFirstTurn);
            if (Raise(nextInOrder.OwnerUid, ev))
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            if (_gameSession.IsPlayer(nextInOrder.OwnerUid))
            {
                if (EntityManager.IsAlive(nextInOrder.OwnerUid))
                {
                    return HandlePlayerTurn(nextInOrder);
                }
                else
                {
                    return TurnOrderState.PlayerDied;
                }
            }
            else
            {
                if (EntityManager.IsAlive(nextInOrder.OwnerUid))
                {
                    return HandleNPCTurn(nextInOrder);
                }
                else
                {
                    return TurnOrderState.PassTurns;
                }
            }
        }

        private TurnOrderState HandlePlayerTurn(TurnOrderComponent turnOrder)
        {
            _field.RefreshScreen();

            var ev = new PlayerTurnStartedEvent();
            RaiseLocalEvent(turnOrder.OwnerUid, ref ev);
            if (ev.Handled)
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            // Now we will now wait for input message handler to inject a turn result into this system.
            return TurnOrderState.PlayerTurnQuery;
        }

        private TurnOrderState HandleNPCTurn(TurnOrderComponent turnOrder)
        {
            var ev = new NPCTurnStartedEvent();
            RaiseLocalEvent(turnOrder.OwnerUid, ref ev);
            return ev.TurnResult.ToTurnOrderState();
        }

        private TurnOrderState DoTurnEnd()
        {
            if (_activeEntity == null || !EntityManager.IsAlive(_activeEntity.OwnerUid))
            {
                return TurnOrderState.PassTurns;
            }

            var ev = new EntityTurnEndingEventArgs();
            RaiseLocalEvent(_activeEntity.OwnerUid, ev);

            return TurnOrderState.PassTurns;
        }

        private TurnOrderState DoPlayerDied()
        {
            _sounds.Play(Protos.Sound.Dead1);
            _field.RefreshScreen();

            var ev = new PlayerDiedEventArgs();
            if (Raise(_gameSession.Player.Uid, ev)) 
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            Mes.Display(Loc.GetString("Elona.Death.GoodBye"));

            var textPrompt = new TextPrompt(maxLength: 16, limitLength: true, prompt: Loc.GetString("Elona.Death.PromptDyingMessage"));
            var result = _uiManager.Query(textPrompt);
            string lastWords;

            if (!result.HasValue || string.IsNullOrEmpty(result.Value))
            {
                lastWords = Loc.GetString("Elona.Death.DefaultLastWords");
            }
            else
            {
                lastWords = result.Value;
            }

            lastWords = Loc.GetString("Elona.Common.Quote", ("str", lastWords));

            Logger.Info($"Player death: {lastWords}");

            return TurnOrderState.TitleScreen;
        }

        #endregion
    }

    internal enum TurnOrderState
    {
        TurnBegin,
        PassTurns,
        TurnEnd,
        PlayerDied,

        PlayerTurnQuery,
        TitleScreen
    }

    internal static class TurnOrderStateExt
    {
        public static TurnOrderState ToTurnOrderState(this TurnResult result, bool isPlayerTurn = false)
        {
            switch (result)
            {
                case TurnResult.NoResult:
                case TurnResult.Aborted:
                    if (isPlayerTurn)
                        return TurnOrderState.PlayerTurnQuery;
                    else
                        return TurnOrderState.TurnEnd;
                case TurnResult.Failed:
                case TurnResult.Succeeded:
                default:
                    return TurnOrderState.TurnEnd; 
            }
        }
    }

    #region Events

    public class MapChangedEventArgs
    {
        public IMap NewMap { get; }
        public IMap? OldMap { get; }

        public MapChangedEventArgs(IMap newMap, IMap? oldMap)
        {
            NewMap = newMap;
            OldMap = oldMap;
        }
    }

    public class BeforeTurnBeginEventArgs : TurnResultEntityEventArgs
    {
    }

    public class EntityTurnStartingEventArgs : TurnResultEntityEventArgs
    {
        public EntityTurnStartingEventArgs(bool isFirstTurn)
        {
            IsFirstTurn = isFirstTurn;
        }

        public bool IsFirstTurn { get; }
    }

    public class EntityTurnEndingEventArgs : TurnResultEntityEventArgs
    {
    }

    internal class PlayerDiedEventArgs : TurnResultEntityEventArgs
    {
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct PlayerTurnStartedEvent
    {
        /// <summary>
        ///     If this message has already been "handled" by a previous system.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        ///     Turn result of this event.
        /// </summary>
        public TurnResult TurnResult { get; set; }

        public void Handle(TurnResult turnResult)
        {
            Handled = true;
            TurnResult = turnResult;
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct NPCTurnStartedEvent
    {
        /// <summary>
        ///     If this message has already been "handled" by a previous system.
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        ///     Turn result of this event.
        /// </summary>
        public TurnResult TurnResult { get; set; }

        public void Handle(TurnResult turnResult)
        {
            Handled = true;
            TurnResult = turnResult;
        }
    }

    #endregion
}

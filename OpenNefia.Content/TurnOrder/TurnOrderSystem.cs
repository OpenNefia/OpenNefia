using OpenNefia.Analyzers;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Charas;
using OpenNefia.Core.Utility;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Home;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.SaveLoad;

namespace OpenNefia.Content.TurnOrder
{
    public interface ITurnOrderSystem : IEntitySystem
    {
        bool PlayerAboutToRespawn { get; }

        /// <summary>
        /// Recalculates the speed value/modifier for this entity.
        /// </summary>
        void RefreshSpeed(EntityUid entity, TurnOrderComponent? turnOrder = null);

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
        void AdvanceState();

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
    /// Then, the initial "turn budget" each character gets is calculated. A larger budget means more actions.
    /// When a turn begins, each character's turn budget is replenished based on their speed. Each turn, every
    /// character in the map is iterated. if a character has a greater budget than the cost per action for the map,
    /// they can take an action and subtract the cost from their budget. Otherwise, their turn is skipped until their 
    /// turn budget is replenished from waiting enough consecutive turns.
    /// </para>
    /// </remarks>
    public sealed partial class TurnOrderSystem : EntitySystem, ITurnOrderSystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapTransferSystem _mapTransfer = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;
        [Dependency] private readonly IHomeSystem _homes = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly ISaveLoadSystem _saveLoad = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;

        private TurnOrderState _state = TurnOrderState.TurnBegin;

        private List<TurnOrderComponent> _entityOrder = new();
        private IEnumerator<TurnOrderComponent>? _currentTurnOrder;
        private TurnOrderComponent? _activeEntity;
        private bool _saveWasLoaded;

        public bool PlayerAboutToRespawn { get; private set; }

        public override void Initialize()
        {
            _mapManager.OnActiveMapChanged += OnActiveMapChanged;
            _saveGameSerializer.OnGameLoaded += OnGameLoaded;

            SubscribeComponent<MapTurnOrderComponent, ActiveMapChangedEvent>(HandleMapChangedTurnOrder, priority: EventPriorities.VeryHigh);
            SubscribeComponent<TurnOrderComponent, EntityTurnEndingEventArgs>(RefreshSpeedOnTurnEnd, priority: EventPriorities.Lowest);
        }

        #region Event Handlers

        private void OnActiveMapChanged(IMap newMap, IMap? oldMap, MapLoadType loadType)
        {
            InitializeState();
        }

        private void OnGameLoaded(ISaveGameHandle save)
        {
            _saveWasLoaded = true;
        }

        private void HandleMapChangedTurnOrder(EntityUid uid, MapTurnOrderComponent mapTurnOrder, ActiveMapChangedEvent args)
        {
            mapTurnOrder.IsFirstTurn = true;

            InitializeMap(args.NewMap);
            _saveWasLoaded = true;
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
        public void InitializeState()
        {
            Logger.InfoS("turn", $"Starting new turn order state for map {_mapManager.ActiveMap?.Id}");

            _state = TurnOrderState.TurnBegin;
            _entityOrder.Clear();
            _currentTurnOrder = null;
            _activeEntity = null;
            _saveWasLoaded = false;
        }

        /// <inheritdoc/>
        public void AdvanceStateFromPlayer(TurnResult turnResult)
        {
            // Initialize the map cleanly if the player has changed maps
            // since the last turn action.
            CheckIfPlayerInMap();

            _state = turnResult.ToTurnOrderState(isPlayerTurn: true);
            AdvanceState();
        }

        /// <summary>
        /// Checks if an event changed the player's current map without setting the active map to the player's, 
        /// and run map init events if so.
        /// </summary>
        private void CheckIfPlayerInMap()
        {
            var player = _gameSession.Player;
            var playerSpatial = EntityManager.GetComponent<SpatialComponent>(player);

            if (_mapManager.ActiveMap != null && playerSpatial.MapID != _mapManager.ActiveMap.Id)
            {
                Logger.WarningS("turn", $"Player is in map {playerSpatial.MapID}, but we are viewing map {_mapManager.ActiveMap.Id}!");
                if (playerSpatial.MapID != MapId.Nullspace && playerSpatial.MapID != MapId.Global)
                {
                    var oldMapId = _mapManager.ActiveMap!.Id;
                    if (_mapManager.MapIsLoaded(oldMapId))
                    {
                        Logger.WarningS("turn", $"Running full map transfer.");
                        _mapTransfer.DoMapTransfer(playerSpatial, _mapManager.GetMap(oldMapId), playerSpatial.Coordinates, MapLoadType.Traveled);
                    }
                    else
                    {
                        Logger.WarningS("turn", $"Map {oldMapId} is not loaded anymore, skipping map transfer.");
                        _mapManager.SetActiveMap(playerSpatial.MapID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void AdvanceState()
        {
            if (_saveWasLoaded)
            {
                InitializeState();
                _field.RefreshScreen();
                return;
            }

            while (true)
            {
                _state = RunStateChange(_state);

                if (_saveWasLoaded)
                {
                    InitializeState();
                    _field.RefreshScreen();
                    break;
                }

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
            if (!_field.IsInGame())
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
            if (!EntityManager.TryGetComponent(_gameSession.Player, out SpatialComponent spatial))
                return new();
            if (spatial.MapID != map.Id)
                return new();

            // Order the player to always go first.
            // TODO: Order allies to go after the player.
            int TurnOrderComparer(TurnOrderComponent arg)
            {
                if (_gameSession.IsPlayer(arg.Owner))
                    return -1;

                return (int)arg.Owner;
            }

            return _lookup.EntityQueryInMap<TurnOrderComponent>(map.Id, includeChildren: false)
                .Where(ent => EntityManager.IsAlive(ent.Owner))
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
                turnOrder.TimeThisTurn += CalculateSpeed(turnOrder.Owner, turnOrder) * playerTimeThisTurn;
            }
        }

        private GameTimeSpan TimeUnitsToSeconds(int timeUnits)
        {
            return GameTimeSpan.FromSeconds(timeUnits / 5 + 1);
        }

        private TurnOrderState DoTurnBegin()
        {
            _activeEntity = null;

            var map = _mapManager.ActiveMap!;
            var player = _gameSession.Player;

            var ev = new MapBeforeTurnBeginEventArgs();
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
                _world.PassTime(TimeUnitsToSeconds(startingTurnTime));
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
                    if (EntityManager.IsAlive(current.Owner))
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
            nextInOrder.TotalTurnsTaken++;

            _activeEntity = nextInOrder;

            var ev = new EntityTurnStartingEventArgs(isFirstTurn);
            if (Raise(nextInOrder.Owner, ev))
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            var ev2 = new EntityPassTurnEventArgs();
            if (Raise(nextInOrder.Owner, ev2))
                return ev2.TurnResult.ToTurnOrderState();

            if (_gameSession.IsPlayer(nextInOrder.Owner))
            {
                if (EntityManager.IsAlive(nextInOrder.Owner))
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
                if (EntityManager.IsAlive(nextInOrder.Owner))
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
            RaiseEvent(turnOrder.Owner, ref ev);
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
            RaiseEvent(turnOrder.Owner, ref ev);
            return ev.TurnResult.ToTurnOrderState();
        }

        private TurnOrderState DoTurnEnd()
        {
            if (_activeEntity == null || !EntityManager.IsAlive(_activeEntity.Owner))
            {
                return TurnOrderState.PassTurns;
            }

            var ev = new EntityTurnEndingEventArgs();
            RaiseEvent(_activeEntity.Owner, ev);

            return TurnOrderState.PassTurns;
        }

        private TurnOrderState DoPlayerDied()
        {
            _sounds.Play(Protos.Sound.Dead1);
            _field.RefreshScreen();

            var ev = new PlayerDiedEventArgs();
            RaiseEvent(_gameSession.Player, ev);
            if (ev.Handled)
            {
                return ev.TurnResult.ToTurnOrderState();
            }

            _mes.Display(Loc.GetString("Elona.PlayerDeath.GoodBye"));

            var promptArgs = new TextPrompt.Args(maxLength: 16, queryText: Loc.GetString("Elona.PlayerDeath.PromptDyingMessage"));
            var result = _uiManager.Query<TextPrompt, TextPrompt.Args, TextPrompt.Result>(promptArgs);
            string lastWords;

            if (!result.HasValue || string.IsNullOrEmpty(result.Value.Text))
            {
                lastWords = Loc.GetString("Elona.PlayerDeath.DefaultLastWords");
            }
            else
            {
                lastWords = result.Value.Text;
            }

            lastWords = Loc.GetString("Elona.Common.Quotes", ("str", lastWords));

            Logger.Info($"Player death: {lastWords}");

            // TODO respawn screen
            if (_playerQuery.YesOrNo("Revive?"))
            {
                RevivePlayer();
                return TurnOrderState.TurnBegin;
            }

            return TurnOrderState.TitleScreen;
        }

        private void RevivePlayer()
        {
            Exception? ex = null;
            try
            {
                PlayerAboutToRespawn = true;
                DoRevivePlayer();
            }
            catch (Exception e)
            {
                ex = e;
                Logger.Error($"Failed to revive player!\n{e}");
            }
            finally
            {
                PlayerAboutToRespawn = false;
            }

            if (ex != null)
                throw ex;
        }

        private void DoRevivePlayer()
        {
            var player = _gameSession.Player;
            DebugTools.Assert(_charas.Revive(player, force: true), "Failed to revive player!");

            bool foundHome = false;
            foreach (var home in _homes.ActiveHomeIDs)
            {
                if (_mapLoader.TryGetOrLoadMap(home, out var map))
                {
                    _mapTransfer.DoMapTransfer(Spatial(player), map, new CenterMapLocation(), MapLoadType.Traveled);
                    foundHome = true;
                    break;
                }
            }

            if (!foundHome)
                Logger.Error("Player had no valid home set!");

            var ev = new PlayerRevivingEvent();
            RaiseEvent(player, ev);

            _refresh.Refresh(player);
            _saveLoad.QueueAutosave();
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

    [EventUsage(EventTarget.Map)]
    public sealed class MapBeforeTurnBeginEventArgs : TurnResultEntityEventArgs
    {
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class EntityTurnStartingEventArgs : TurnResultEntityEventArgs
    {
        public EntityTurnStartingEventArgs(bool isFirstTurn)
        {
            IsFirstTurn = isFirstTurn;
        }

        public bool IsFirstTurn { get; }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class EntityPassTurnEventArgs : TurnResultEntityEventArgs
    {
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class EntityTurnEndingEventArgs : TurnResultEntityEventArgs
    {
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class PlayerDiedEventArgs : TurnResultEntityEventArgs
    {
    }

    [ByRefEvent]
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

    [ByRefEvent]
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

    public sealed class PlayerRevivingEvent : EntityEventArgs {}

    #endregion
}

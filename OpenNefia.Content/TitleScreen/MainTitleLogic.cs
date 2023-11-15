using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Areas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.CharaMake;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Areas;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Content.CustomName;
using OpenNefia.Content.SaveLoad;
using OpenNefia.Core.Log;
using OpenNefia.Content.RandomText;
using OpenNefia.Content.Scenarios;
using OpenNefia.Core.EngineVariables;

namespace OpenNefia.Content.TitleScreen
{
    public interface IMainTitleLogic
    {
        void RunTitleScreen();
    }

    public class MainTitleLogic : IMainTitleLogic
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSessionManager = default!;
        [Dependency] private readonly IFieldLayer _fieldLayer = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ICharaMakeLogic _charaMakeLogic = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRandomAliasGenerator _randomAlias = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;

        private void Startup()
        {
            _fieldLayer.Startup();
        }

        public void RunTitleScreen()
        {
            Startup();

            var action = TitleScreenAction.ReturnToTitle;

            if (_config.GetCVar(CCVars.DebugQuickstartOnStartup))
            {
                try
                {
                    RunQuickStart(new(_config.GetCVar(CCVars.DebugQuickstartScenario)));
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("maintitle", ex, $"Quickstart failed");
                }
            }

            while (action != TitleScreenAction.Quit)
            {
                _saveGameSerializer.ResetGameState();

                using (ITitleScreenLayer titleScreen = _uiManager.CreateLayer<TitleScreenLayer, TitleScreenResult>())
                {
                    var bg = new TitleScreenBGLayer();
                    _uiManager.PushLayer(bg);
                    var result = _uiManager.Query(titleScreen);
                    _uiManager.PopLayer(bg);

                    Console.WriteLine(result);

                    if (result.HasValue)
                    {
                        try
                        {
                            action = result.Value.Action;
                            switch (action)
                            {
                                case TitleScreenAction.ReturnToTitle:
                                    break;
                                case TitleScreenAction.RestoreSave:
                                    RunRestoreSave();
                                    break;
                                case TitleScreenAction.Generate:
                                    RunGenerate();
                                    break;
                                case TitleScreenAction.Options:
                                    _uiManager.PushLayer(bg);
                                    ShowConfigMenu();
                                    _uiManager.PopLayer(bg);
                                    break;
                                case TitleScreenAction.Quit:
                                    break;
                                case TitleScreenAction.QuickStart:
                                    RunQuickStart();
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorS("maintitle", $"Exception when running action {action}:\n{ex}");
                            action = TitleScreenAction.ReturnToTitle;
                        }
                    }
                    else
                    {
                        action = TitleScreenAction.ReturnToTitle;
                    }
                }
            }
        }

        [EngineVariable("Elona.QuickstartScenarios")]
        private List<PrototypeId<ScenarioPrototype>> _varQuickstartScenarios { get; } = new();

        private void RunQuickStart(PrototypeId<ScenarioPrototype>? scenarioId = null)
        {
            if (scenarioId == null)
            {
                if (_varQuickstartScenarios.Count == 0)
                {
                    return;
                }
                else if (_varQuickstartScenarios.Count == 1)
                {
                    scenarioId = _varQuickstartScenarios.First();
                }
                else
                {
                    scenarioId = _playerQuery.PickOrNoneS(_varQuickstartScenarios);
                    if (scenarioId == null)
                        return;
                }
            }

            _saveGameSerializer.ResetGameState();
            var layer = _uiManager.CreateLayer<CharaMakeCharaSheetLayer>();
            var player = layer.CreatePlayerEntity(new List<ICharaMakeResult>()
            {
                new CharaMakeClassSelectLayer.ResultData(Protos.Class.Predator),
                new CharaMakeRaceSelectLayer.ResultData(Protos.Race.Machinegod),
                new CharaMakeAliasLayer.ResultData(_randomAlias.GenerateRandomAlias(AliasType.Chara)),
            });
            var customName = _entityManager.EnsureComponent<CustomNameComponent>(player);
            customName.CustomName = "*QuickStart*";

            // Wipe the previous quickstart save(s)
            foreach (var save in _saveGameManager.AllSaves.ToList())
            {
                if (save.Header.Name == "*QuickStart*")
                {
                    _saveGameManager.DeleteSave(save);
                }
            }
            StartNewGame(player, scenarioId.Value);
        }

        private void RunRestoreSave()
        {
            var result = _uiManager.Query<RestoreSaveLayer, RestoreSaveLayer.Result>();

            if (result.HasValue)
            {
                LoadGame(result.Value.SaveGame);
            }
        }

        private void RunGenerate()
        {
            var result = _charaMakeLogic.RunCreateChara();

            if (result is CharaMakeLogicResult.NewPlayerIncarnated newPlayerResult)
            {
                StartNewGame(newPlayerResult.NewPlayer, newPlayerResult.ScenarioID);
            }
        }

        private void ShowConfigMenu()
        {
            ConfigMenuHelpers.QueryDefaultConfigMenu(_protos, _uiManager, _config);
        }

        private void StartNewGame(EntityUid player, PrototypeId<ScenarioPrototype> scenarioID)
        {
            var saveName = EntitySystem.Get<IDisplayNameSystem>().GetDisplayName(player);
            var scenarioProto = _protos.Index(scenarioID);

            var save = _saveGameSerializer.InitializeSaveGame(saveName);
            _saveGameManager.CurrentSave = save;

            _gameSessionManager.Player = player;

            EntitySystem.Get<IGlobalAreaSystem>().InitializeGlobalAreas(scenarioProto.LoadGlobalAreas);

            var pev = new P_ScenarioOnGameStartEvent(player);
            _protos.EventBus.RaiseEvent(scenarioID, pev);
            
            if (pev.OutActiveMap == null)
            {
                throw new InvalidDataException($"Scenario {scenarioID} did not return an active map");
            }

            var map = pev.OutActiveMap;
            _mapManager.SetActiveMap(map.Id);

            var ev = new NewGameStartedEventArgs();
            _entityManager.EventBus.RaiseEvent(_gameSessionManager.Player, ev);

            _mapManager.RefreshVisibility(map);

            // copied from CommonCommandsSystem
            _saveGameSerializer.SaveGame(save);
            Sounds.Play(Protos.Sound.Write1);
            _mes.Display(Loc.GetString("Elona.UserInterface.Save.QuickSave"));

            QueryFieldLayer();
        }

        private void LoadGame(ISaveGameHandle saveGame)
        {
            _saveGameManager.CurrentSave = saveGame;

            _saveGameSerializer.LoadGame(saveGame);
            var map = _mapManager.ActiveMap!;

            _mapManager.RefreshVisibility(map);

            QueryFieldLayer();

            _mapManager.UnloadMap(map.Id);

            _saveGameManager.CurrentSave = null;
        }

        private void QueryFieldLayer()
        {
            _hud.Initialize();
            var hudLayer = (UiLayer)_hud;
            hudLayer.ZOrder = HudLayer.HudZOrder;
            _uiManager.PushLayer(hudLayer);

            var ev1 = new GameInitiallyLoadedEventArgs();
            _entityManager.EventBus.RaiseEvent(ev1);

            var ev2 = new GameQuickLoadedEventArgs();
            _entityManager.EventBus.RaiseEvent(ev2);

            _uiManager.Query(_fieldLayer);
            _hud.ClearWidgets();
            _uiManager.PopLayer(hudLayer);

            var evCleanedUp = new GameCleanedUpEventArgs();
            _entityManager.EventBus.RaiseEvent(evCleanedUp);
        }
    }

    /// <summary>
    /// Raised when the game is first loaded from the title screen, *not* when
    /// the game is quickloaded while already in-game.
    /// </summary>
    public sealed class GameInitiallyLoadedEventArgs : EntityEventArgs
    {
        public GameInitiallyLoadedEventArgs()
        {
        }
    }

    /// <summary>
    /// Raised when the game is first loaded from the title screen or quickloaded.
    /// </summary>
    public sealed class GameQuickLoadedEventArgs : EntityEventArgs
    {
        public GameQuickLoadedEventArgs()
        {
        }
    }

    /// <summary>
    /// Raised when a new save file is created and the game is about to start.
    /// </summary>
    public sealed class NewGameStartedEventArgs : EntityEventArgs
    {
        public NewGameStartedEventArgs()
        {
        }
    }

    /// <summary>
    /// Raised when the player quits the game, either back to the title screen or to the desktop.
    /// </summary>
    public sealed class GameCleanedUpEventArgs : EntityEventArgs
    {
    }
}

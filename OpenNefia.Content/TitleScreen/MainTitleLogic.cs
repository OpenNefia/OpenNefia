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
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ICharaMakeLogic _charaMakeLogic = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        private void Startup()
        {
            _fieldLayer.Startup();
        }

        public void RunTitleScreen()
        {
            Startup();

            var action = TitleScreenAction.ReturnToTitle;

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

        private void RunQuickStart()
        {
            _saveGameSerializer.ResetGameState();
            var layer = _uiManager.CreateLayer<CharaMakeCharaSheetLayer>();
            var player = layer.CreatePlayerEntity(new List<ICharaMakeResult>()
            {
                new CharaMakeRaceSelectLayer.ResultData(Protos.Race.God),
            });
            var customName = _entityManager.EnsureComponent<CustomNameComponent>(player);
            customName.CustomName = "*QuickStart*";
            StartNewGame(player);
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
                StartNewGame(newPlayerResult.NewPlayer);
            }
        }

        private void ShowConfigMenu()
        {
            ConfigMenuHelpers.QueryDefaultConfigMenu(_prototypeManager, _uiManager, _config);
        }

        /// <summary>
        /// Does one-time setup of global areas. This is for setting up areas like towns to
        /// be able to generate escort/other quests between them when a new save is being 
        /// initialized.
        /// </summary>
        public void InitializeGlobalAreas()
        {
            DebugTools.Assert(_areaManager.LoadedAreas.Count == 0, "Areas were already initialized!");

            foreach (var (areaEntityProto, globalAreaId) in EnumerateGlobalAreas())
            {
                var areaId = areaEntityProto.GetStrongID();
                _areaManager.CreateArea(areaId, globalAreaId);
            }
        }

        private IEnumerable<(EntityPrototype, GlobalAreaId)> EnumerateGlobalAreas()
        {
            foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
            {
                if (proto.TryGetComponent<AreaEntranceComponent>("AreaEntrance", out var areaEntrance))
                {
                    if (areaEntrance.GlobalId != null)
                    {
                        yield return (proto, areaEntrance.GlobalId.Value);
                    }
                }
            }
        }

        private void StartNewGame(EntityUid player)
        {
            var saveName = EntitySystem.Get<IDisplayNameSystem>().GetBaseName(player);

            var save = _saveGameSerializer.InitializeSaveGame(saveName);
            _saveGameManager.CurrentSave = save;

            _gameSessionManager.Player = player;

            InitializeGlobalAreas();

            var map = _mapLoader.LoadBlueprint(new ResourcePath("/Maps/LecchoTorte/Test.yml"));
            map.MemorizeAllTiles();

            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(player);
            playerSpatial.Coordinates = map.AtPosEntity(2, 2);

            _mapManager.SetActiveMap(map.Id);

            var ev = new NewGameStartedEventArgs();
            _entityManager.EventBus.RaiseEvent(_gameSessionManager.Player, ev);

            _mapManager.RefreshVisibility(map);

            // copied from CommonCommandsSystem
            _saveGameSerializer.SaveGame(save);
            Sounds.Play(Protos.Sound.Write1);
            _mes.Display(Loc.GetString("Elona.UserInterface.Save.QuickSave"));

            QueryFieldLayer();

            _saveGameManager.CurrentSave = null;
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

            var ev = new GameInitiallyLoadedEventArgs();
            _entityManager.EventBus.RaiseEvent(ev);

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
    /// Raised when a new save file is created and the game is about to start.
    /// </summary>
    public sealed class NewGameStartedEventArgs : EntityEventArgs
    {
        public NewGameStartedEventArgs()
        {
        }
    }

    /// <summary>
    /// Raised when the player quits the game, either back to the title screen or to desktop.
    /// </summary>
    public sealed class GameCleanedUpEventArgs : EntityEventArgs
    {
    }
}

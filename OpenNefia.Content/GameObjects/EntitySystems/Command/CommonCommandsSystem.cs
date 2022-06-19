using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.ConfigMenu;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Input;
using OpenNefia.Content.Journal;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Content.CharaInfo.CharaGroupSublayerArgs;
using static OpenNefia.Content.Journal.LogGroupSublayerArgs;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.GameObjects
{
    public class CommonCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                // CharaInfo group
                .Bind(ContentKeyFunctions.CharaInfo, InputCmdHandler.FromDelegate(ShowCharaInfo))
                .Bind(ContentKeyFunctions.Equipment, InputCmdHandler.FromDelegate(ShowEquipment))
                .Bind(ContentKeyFunctions.FeatInfo, InputCmdHandler.FromDelegate(ShowFeatInfo))

                // Journal group
                .Bind(ContentKeyFunctions.Backlog, InputCmdHandler.FromDelegate(ShowBacklog))
                .Bind(ContentKeyFunctions.Journal, InputCmdHandler.FromDelegate(ShowJournal))
                .Bind(ContentKeyFunctions.ChatLog, InputCmdHandler.FromDelegate(ShowChatLog))
                
                // Other commands
                .Bind(EngineKeyFunctions.ShowEscapeMenu, InputCmdHandler.FromDelegate(ShowEscapeMenu))
                .Bind(EngineKeyFunctions.QuickSaveGame, InputCmdHandler.FromDelegate(QuickSaveGame))
                .Bind(EngineKeyFunctions.QuickLoadGame, InputCmdHandler.FromDelegate(QuickLoadGame))
                .Register<CommonCommandsSystem>();
        }

        private TurnResult? ShowCharaInfo(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            var context = new CharaUiGroupArgs(CharaTab.CharaInfo, session.Player);
            var result = _uiManager.Query<CharaUiGroup, CharaUiGroupArgs, CharaGroupSublayerResult>(context);

            return HandleCharaUiGroupResult(result);
        }

        private TurnResult? ShowEquipment(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            var context = new CharaUiGroupArgs(CharaTab.Equipment, session.Player);
            var result = _uiManager.Query<CharaUiGroup, CharaUiGroupArgs, CharaGroupSublayerResult>(context);

            return HandleCharaUiGroupResult(result);
        }

        private TurnResult? ShowFeatInfo(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            var context = new CharaUiGroupArgs(CharaTab.FeatInfo, session.Player);
            var result = _uiManager.Query<CharaUiGroup, CharaUiGroupArgs, CharaGroupSublayerResult>(context);

            return HandleCharaUiGroupResult(result);
        }

        private TurnResult HandleCharaUiGroupResult(UiResult<CharaGroupSublayerResult> result)
        {
            if (result.HasValue && result.Value.ChangedEquipment)
            {
                _mes.Display(Loc.GetString("Elona.Equipment.YouChangeYourEquipment"));
                return TurnResult.Succeeded;
            }

            return TurnResult.Aborted;
        }

        private TurnResult? ShowJournal(IGameSessionManager? session)
        {
            var context = new LogUiGroupArgs(LogTab.Journal);
            _uiManager.Query<LogUiGroup, LogUiGroupArgs>(context);

            return TurnResult.Aborted;
        }

        private TurnResult? ShowBacklog(IGameSessionManager? session)
        {
            var context = new LogUiGroupArgs(LogTab.Backlog);
            _uiManager.Query<LogUiGroup, LogUiGroupArgs>(context);

            return TurnResult.Aborted;
        }

        private TurnResult? ShowChatLog(IGameSessionManager? session)
        {
            var context = new LogUiGroupArgs(LogTab.ChatLog);
            _uiManager.Query<LogUiGroup, LogUiGroupArgs>(context);

            return TurnResult.Aborted;
        }

        private TurnResult? QuickSaveGame(IGameSessionManager? session)
        {
            var save = _saveGameManager.CurrentSave!;

            // Step one frame to get a screenshot without modals.
            _gameController.StepFrame();

            // Put the screenshot in the temp files first.
            SaveScreenshot(save);

            // Commit all temp files.
            _saveGameSerializer.SaveGame(save);

            _sounds.Play(Sound.Write1);
            _mes.Display(Loc.GetString("Elona.UserInterface.Save.QuickSave"));

            return TurnResult.Aborted;
        }

        private void SaveScreenshot(ISaveGameHandle save)
        {
            var path = SaveGameConstants.ScreenshotPath;

            // This will output a capture of the previous draw frame, as we're in the middle
            // of the next frame's Update() call.
            var pngBytes = _graphics.CaptureCanvasPNG();

            save.Files.WriteAllBytes(path, pngBytes);
        }

        private TurnResult? QuickLoadGame(IGameSessionManager? session)
        {
            var save = _saveGameManager.CurrentSave!;

            _saveGameSerializer.LoadGame(save);

            return TurnResult.Aborted;
        }

        private enum EscapeMenuChoice
        {
            Cancel,
            GameSetting,
            ReturnToTitle,
            Exit,
        }

        private TurnResult? ShowEscapeMenu(IGameSessionManager? session)
        {
            if (!_field.IsInGame() || session?.Player == null)
                return null;

            var keyRoot = new LocaleKey("Elona.UserInterface.Exit.Prompt.Choices");
            var choices = new PromptChoice<EscapeMenuChoice>[]
            {
#pragma warning disable format
                new(EscapeMenuChoice.Cancel,        keyRoot.With(nameof(EscapeMenuChoice.Cancel))),
                new(EscapeMenuChoice.GameSetting,   keyRoot.With(nameof(EscapeMenuChoice.GameSetting))),
                new(EscapeMenuChoice.ReturnToTitle, keyRoot.With(nameof(EscapeMenuChoice.ReturnToTitle))),
                new(EscapeMenuChoice.Exit,          keyRoot.With(nameof(EscapeMenuChoice.Exit)))
#pragma warning restore format
            };

            var promptArgs = new Prompt<EscapeMenuChoice>.Args(choices)
            {
                QueryText = Loc.GetString("Elona.UserInterface.Exit.Prompt.Text")
            };

            var result = _uiManager.Query<Prompt<EscapeMenuChoice>, 
                Prompt<EscapeMenuChoice>.Args, 
                PromptChoice<EscapeMenuChoice>>(promptArgs);

            if (result.HasValue)
            {
                switch (result.Value.ChoiceData)
                {
                    case EscapeMenuChoice.GameSetting:
                        ShowConfigMenu();
                        break;
                    case EscapeMenuChoice.ReturnToTitle:
                        ReturnToTitle(session!);
                        break;
                    case EscapeMenuChoice.Exit:
                        ExitGame(session!);
                        break;
                    case EscapeMenuChoice.Cancel:
                    default:
                        break;
                }
            }

            return TurnResult.Aborted;
        }

        private void ShowConfigMenu()
        {
            Sounds.Play(Sound.Ok1);
            ConfigMenuHelpers.QueryDefaultConfigMenu(_protos, _uiManager, _config);
        }

        private void ReturnToTitle(IGameSessionManager gameSession)
        {
            QuickSaveGame(gameSession);
            _mes.Display(Loc.GetString("Elona.UserInterface.Exit.Saved"));
            _playerQuery.PromptMore();
            _gameController.Wait(0.3f);

            _field.Cancel();
        }

        private void ExitGame(IGameSessionManager gameSession)
        {
            QuickSaveGame(gameSession);
            _mes.Display(Loc.GetString("Elona.UserInterface.Exit.Saved"));
            _mes.Display(Loc.GetString("Elona.UserInterface.Exit.YouCloseYourEyes", ("entity", gameSession.Player!)));
            _playerQuery.PromptMore();
            _gameController.Wait(0.3f);

            _gameController.Shutdown();
        }
    }
}

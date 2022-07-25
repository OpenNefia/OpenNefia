using OpenNefia.Content.GameController;
using OpenNefia.Content.Logic;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Audio;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.SaveGames;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.SaveLoad
{
    public interface ISaveLoadSystem : IEntitySystem
    {
        void QueueAutosave(AutosaveType threshold = AutosaveType.Always);
        void QuickSaveGame();
        void QuickLoadGame();
    }

    public sealed class SaveLoadSystem : EntitySystem, ISaveLoadSystem
    {
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public void QueueAutosave(AutosaveType type = AutosaveType.Always)
        {
            // TODO
        }

        public void QuickSaveGame()
        {
            var save = _saveGameManager.CurrentSave!;

            // Step one frame to get a screenshot without modals.
            _gameController.StepFrame();

            // Put the screenshot in the temp files first.
            SaveScreenshot(save);

            // Commit all temp files.
            _saveGameSerializer.SaveGame(save);

            _audio.Play(Sound.Write1);
            _mes.Display(Loc.GetString("Elona.UserInterface.Save.QuickSave"));
        }

        public void QuickLoadGame()
        {
            var save = _saveGameManager.CurrentSave!;

            _saveGameSerializer.LoadGame(save);

            var ev = new GameQuickLoadedEventArgs();
            RaiseEvent(ev);
        }

        private void SaveScreenshot(ISaveGameHandle save)
        {
            var path = SaveGameConstants.ScreenshotPath;

            // This will output a capture of the previous draw frame, as we're in the middle
            // of the next frame's Update() call.
            var pngBytes = _graphics.CaptureCanvasPNG();

            save.Files.WriteAllBytes(path, pngBytes);
        }
    }
}

using OpenNefia.Core.IoC;
using OpenNefia.Core.SaveGames;

namespace OpenNefia.Content.World
{
    public interface IPlayTimeManager
    {
        /// <summary>
        /// Amount of real time played at the last time the game was saved.
        /// </summary>
        TimeSpan PlayTimeAtLastSave { get; }

        /// <summary>
        /// Amount of real time the player has played the game.
        /// </summary>
        TimeSpan PrecisePlayTime { get; }

        void Initialize();
        void UpdateLastSaveTime();
    }

    public sealed class PlayTimeManager : IPlayTimeManager
    {
        [Dependency] private readonly ISaveGameSerializer _saveGameSerializer = default!;

        private DateTime _begin;

        /// <inheritdoc/>
        [RegisterSaveData("Elona.PlayTimeManager.PlayTimeAtLastSave")]
        public TimeSpan PlayTimeAtLastSave { get; private set; } = TimeSpan.Zero;

        /// <inheritdoc/>
        public TimeSpan PrecisePlayTime => (DateTime.Now - _begin) + PlayTimeAtLastSave;

        public void Initialize()
        {
            _begin = DateTime.Now;
            _saveGameSerializer.BeforeGameSaved += BeforeGameSaved;
            _saveGameSerializer.OnGameLoaded += OnGameLoaded;
        }

        private void OnGameLoaded(ISaveGameHandle save)
        {
            _begin = DateTime.Now;
        }

        private void BeforeGameSaved(ISaveGameHandle save)
        {
            UpdateLastSaveTime();
        }

        public void UpdateLastSaveTime()
        {
            var now = DateTime.Now;
            PlayTimeAtLastSave = PrecisePlayTime;
            _begin = DateTime.Now;
        }
    }
}

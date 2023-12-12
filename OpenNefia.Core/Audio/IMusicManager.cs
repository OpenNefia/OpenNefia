using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public interface IMusicManager
    {
        public bool IsPlaying { get; }

        public void Initialize();
        public void Shutdown();

        IEnumerable<OutputDevice> GetMidiOutputDevices();

        /// <summary>
        /// Plays a music file.
        /// </summary>
        /// <param name="musicId">Prototype of the music to play.</param>
        public void Play(PrototypeId<MusicPrototype> musicId, bool loop = true);

        /// <summary>
        /// Restarts playing the current music.
        /// </summary>
        public void Restart();

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void Stop();
    }
}

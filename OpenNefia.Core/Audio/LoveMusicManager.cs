using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Config;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public sealed class LoveMusicManager : IMusicManager
    {
        private static Playback? MidiPlayback = null;
        private static OutputDevice? MidiDevice = null;

        private static OutputDevice GetMidiOutputDevice() => OutputDevice.GetByIndex(0);

        public bool IsPlaying => MidiPlayback != null;

        /// <inheritdoc />
        public void Play(PrototypeId<MusicPrototype> id)
        {
            if (IsPlaying)
                Stop();

            if (!ConfigVars.EnableMusic)
                return;

            var path = id.ResolvePrototype().Filepath;

            if (path.Extension == "mid")
            {
                var midiFile = MidiFile.Read(path.ToString());

                MidiDevice = GetMidiOutputDevice();
                MidiPlayback = midiFile.GetPlayback(MidiDevice);
                MidiPlayback.Loop = true;
                MidiPlayback.Start();
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (MidiPlayback != null)
            {
                MidiPlayback.Dispose();
                MidiPlayback = null;
            }
            if (MidiDevice != null)
            {
                MidiDevice.Dispose();
                MidiDevice = null;
            }
        }
    }
}

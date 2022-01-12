using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public sealed class LoveMusicManager : IMusicManager
    {
        [Dependency] private readonly IConfigurationManager _config = default!;

        private static Playback? MidiPlayback = null;
        private static OutputDevice? MidiDevice = null;

        private static OutputDevice GetMidiOutputDevice() => OutputDevice.GetByIndex(0);

        public bool IsPlaying => MidiPlayback != null;

        private PrototypeId<MusicPrototype>? _currentlyPlaying;

        private bool _enableMusic;

        public void Initialize()
        {
            _config.OnValueChanged(CVars.AudioMusic, OnConfigEnableMusicChanged, true);
        }

        private void OnConfigEnableMusicChanged(bool b)
        {
            _enableMusic = b;
            if (!_enableMusic)
                StopInternal();
            else if (_currentlyPlaying != null)
                Play(_currentlyPlaying.Value);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<MusicPrototype> id)
        {
            if (IsPlaying)
                StopInternal();

            _currentlyPlaying = id;

            if (!_enableMusic)
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
            _currentlyPlaying = null;
        }

        private void StopInternal()
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

using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Audio
{
    public sealed class LoveMusicManager : IMusicManager
    {
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private const string MidiFileExtension = "mid";

        private OutputDevice _midiDevice = default!;
        private Playback? _midiPlayback = null;
        private Love.Source? _streamPlayback = null;

        private PrototypeId<MusicPrototype>? _currentlyPlaying;

        public bool IsPlaying => _midiPlayback != null || _streamPlayback != null;

        private bool _enableMusic;

        public void Initialize()
        {
            _config.OnValueChanged(CVars.AudioMidiDevice, i => _midiDevice = OutputDevice.GetByIndex(i), true);
            _config.OnValueChanged(CVars.AudioMusic, OnConfigEnableMusicChanged, true);
        }

        public void Shutdown()
        {
            Stop();
            _midiDevice?.Dispose();
        }

        private void OnConfigEnableMusicChanged(bool b)
        {
            _enableMusic = b;
            if (!_enableMusic)
                StopInternal();
            else if (_currentlyPlaying != null)
                Play(_currentlyPlaying.Value);
        }

        private Love.Source GetLoveStreamSource(ResourcePath path)
        {
            var fileData = _resourceCache.GetResource<LoveFileDataResource>(path);
            return Love.Audio.NewSource(fileData, Love.SourceType.Stream);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<MusicPrototype> musicId)
        {
            if (IsPlaying)
                StopInternal();

            _currentlyPlaying = musicId;

            if (!_enableMusic)
                return;

            var path = _protos.Index(musicId).Filepath;

            if (path.Extension.Equals(MidiFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                using (var stream = _resourceCache.ContentFileRead(path))
                {
                    var midiFile = MidiFile.Read(stream);

                    _midiPlayback = midiFile.GetPlayback(_midiDevice);
                    _midiPlayback.Loop = true;
                    _midiPlayback.Start();
                }
            }
            else
            {
                _streamPlayback = GetLoveStreamSource(path);
                _streamPlayback.SetLooping(true);
                Love.Audio.Play(_streamPlayback);
            }
        }

        /// <inheritdoc />
        public void Stop()
        {
            _currentlyPlaying = null;
            StopInternal();
        }

        private void StopInternal()
        {
            _midiPlayback?.Dispose();

            if (_streamPlayback != null)
            {
                Love.Audio.Stop(_streamPlayback);
                _streamPlayback.Dispose();
            }

            _midiPlayback = null;
            _streamPlayback = null;
        }
    }
}

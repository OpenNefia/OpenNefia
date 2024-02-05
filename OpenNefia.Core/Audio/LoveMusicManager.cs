using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
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

        private OutputDevice? _midiDevice = default!;
        private Playback? _midiPlayback = null;
        private Love.Source? _streamPlayback = null;

        private PrototypeId<MusicPrototype>? _lastPlayedMusic;
        public PrototypeId<MusicPrototype>? PlayingMusicID => _lastPlayedMusic;

        public bool IsPlaying => _midiPlayback != null || _streamPlayback != null;

        private bool _enableMusic;
        private float _volume;

        public void Initialize()
        {
            _config.OnValueChanged(CVars.AudioMidiDevice, OnConfigMidiDeviceChanged, true);
            _config.OnValueChanged(CVars.AudioMusic, OnConfigEnableMusicChanged, true);
            _config.OnValueChanged(CVars.AudioDeviceVolume, OnConfigDeviceVolumeChanged, true);
        }

        public void Shutdown()
        {
            Stop();
            _midiDevice?.Dispose();
            _midiDevice = null;
        }

        public IEnumerable<OutputDevice> GetMidiOutputDevices()
        {
            try {
                // Eager consume enumerable so native library load runs inside this block.
                return OutputDevice.GetAll().ToList();
            }
            catch (Exception ex) {
                // Native library failed to load.
                Logger.ErrorS("music", ex, $"Failed to load native MIDI library.");
                return Enumerable.Empty<OutputDevice>();
            }
        }

        private void OnConfigMidiDeviceChanged(int deviceIndex)
        {
            StopInternal();
            _midiDevice?.Dispose();
            _midiDevice = null;

            try
            {
                _midiDevice = OutputDevice.GetByIndex(deviceIndex);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("music", ex, $"Failed to find MIDI output device {deviceIndex}!");
            }

            Restart();
        }

        private void OnConfigEnableMusicChanged(bool b)
        {
            _enableMusic = b;
            Restart();
        }
        private void OnConfigDeviceVolumeChanged(float volume)
        {
            _volume = volume;
            Restart();
        }

        private Love.Source GetLoveStreamSource(ResourcePath path)
        {
            var fileData = _resourceCache.GetResource<LoveFileDataResource>(path);
            return Love.Audio.NewSource(fileData, Love.SourceType.Stream);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<MusicPrototype> musicId, bool loop = true)
        {
            if (IsPlaying && _lastPlayedMusic == musicId)
                return;

            if (IsPlaying)
                StopInternal();

            _lastPlayedMusic = musicId;

            if (!_enableMusic)
                return;

            var path = _protos.Index(musicId).Filepath;

            if (path.Extension.Equals(MidiFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                if (_midiDevice != null)
                {
                    using (var stream = _resourceCache.ContentFileRead(path))
                    {
                        var midiFile = MidiFile.Read(stream);

                        _midiPlayback = midiFile.GetPlayback(_midiDevice);
                        _midiPlayback.Loop = loop;
                        VolumeHelper.SetVolume(_volume*100);
                        _midiPlayback.Start();
                    }
                }
            }
            else
            {
                _streamPlayback = GetLoveStreamSource(path);
                _streamPlayback.SetLooping(loop);
                Love.Audio.Play(_streamPlayback);
            }
        }

        public void Restart()
        {
            StopInternal();
            if (_lastPlayedMusic != null)
                Play(_lastPlayedMusic.Value);
        }

        /// <inheritdoc />
        public void Stop()
        {
            _lastPlayedMusic = null;
            StopInternal();
        }

        private void StopInternal()
        {
            _midiPlayback?.Dispose();
            _midiDevice?.TurnAllNotesOff();

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

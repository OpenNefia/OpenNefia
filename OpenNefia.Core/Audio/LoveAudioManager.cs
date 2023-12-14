using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Audio
{
    public sealed class LoveAudioManager : IAudioManager
    {
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private bool _enableSound;
        private bool _usePositionalSound;
        private Dictionary<string, PlayingSource> _loopingSources = new();

        private class PlayingSource
        {
            public Love.Source Source;

            public PlayingSource(Love.Source source)
            {
                Source = source;
            }
        }

        private List<PlayingSource> _playingSources = new();

        public void Initialize()
        {
            _config.OnValueChanged(CVars.AudioSound, b => _enableSound = b, true);
            _config.OnValueChanged(CVars.AudioPositionalAudio, b => _usePositionalSound = b, true);
        }

        public void FrameUpdate(FrameEventArgs frame)
        {
            for (var i = 0; i < _playingSources.Count; i++)
            {
                var source = _playingSources[i];
                if (!source.Source.IsPlaying())
                {
                    source.Source.Dispose();
                    _playingSources.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Shutdown()
        {
            foreach (var source in _playingSources)
            {
                Love.Audio.Stop(source.Source);
                source.Source.Dispose();
            }
            _playingSources.Clear();

            StopAllLooping();
        }

        private Love.Source GetLoveSource(ResourcePath path)
        {
            var fileData = _resourceCache.GetResource<LoveFileDataResource>(path);
            return Love.Audio.NewSource(fileData, Love.SourceType.Static);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> soundId, AudioParams? audioParams = null)
        {
            if (!_enableSound)
                return;

            var proto = _protos.Index(soundId);
            var source = GetLoveSource(proto.Filepath);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(true);
                source.SetAttenuationDistances(0, 0);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> soundId, EntityUid entity, AudioParams? audioParams = null)
        {
            if (_entityManager.TryGetComponent<SpatialComponent>(entity, out var spatial))
                Play(soundId, spatial.MapPosition, audioParams);
            else
                Play(soundId);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> soundId, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
            if (coordinates.MapId != _mapManager.ActiveMap?.Id)
                return;

            var screenPosition = _coords.TileToScreen(coordinates.Position);

            Play(soundId, screenPosition, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> soundId, EntityCoordinates coordinates, AudioParams? audioParams = null)
        {
            Play(soundId, coordinates.ToMap(_entityManager), audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> soundId, Vector2i screenPosition, AudioParams? audioParams = null)
        {
            if (!_enableSound)
                return;

            var proto = _protos.Index(soundId);
            var source = GetLoveSource(proto.Filepath);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(false);

                if (_usePositionalSound)
                {
                    source.SetPosition(screenPosition.X, screenPosition.Y, 0f);
                    source.SetAttenuationDistances(100, 500);
                }
                else
                {
                    source.SetAttenuationDistances(0, 0);
                }
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        public void SetListenerPosition(Vector2 listenerPos)
        {
            Love.Audio.SetPosition(listenerPos.X, listenerPos.Y, 0f);
        }

        public void PlayLooping(PrototypeId<SoundPrototype> soundId, string tag, AudioParams? audioParams = null)
        {
            if (_loopingSources.ContainsKey(tag))
                StopLooping(tag);

            var proto = _protos.Index(soundId);
            var source = GetLoveSource(proto.Filepath);

            source.SetLooping(true);

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _loopingSources[tag] = new PlayingSource(source);
        }

        public void StopLooping(string tag)
        {
            if (!_loopingSources.TryGetValue(tag, out var source))
                return;

            Love.Audio.Stop(source.Source);
            source.Source.Dispose();
            _loopingSources.Remove(tag);
        }

        public void StopAllLooping()
        {
            foreach (var tag in _loopingSources.Keys.ToList())
            {
                StopLooping(tag);
            }
            _loopingSources.Clear();
        }
    }
}

using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
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
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private bool _enableSound;
        private bool _usePositionalSound;

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
            _config.OnValueChanged(CVars.AudioPositionalSound, b => _usePositionalSound = b, true);
        }

        private Love.Source GetLoveSource(PrototypeId<SoundPrototype> prototype)
        {
            var fileData = _resourceCache.GetResource<LoveFileDataResource>(prototype.ResolvePrototype().Filepath);
            return Love.Audio.NewSource(fileData, Love.SourceType.Static);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null)
        {
            if (!_enableSound)
                return;

            var source = GetLoveSource(prototype);

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
        public void Play(PrototypeId<SoundPrototype> prototype, EntityUid entity, AudioParams? audioParams = null)
        {
            if (_entityManager.TryGetComponent<SpatialComponent>(entity, out var spatial))
                Play(prototype, spatial.MapPosition, audioParams);
            else
                Play(prototype);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
            if (coordinates.MapId != _mapManager.ActiveMap?.Id)
                return;

            var screenPosition = _coords.TileToScreen(coordinates.Position);

            Play(prototype, screenPosition, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, Vector2i screenPosition, AudioParams? audioParams = null)
        {
            if (!_enableSound)
                return;

            var source = GetLoveSource(prototype);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(false);
                source.SetAttenuationDistances(100, 500);

                if (_usePositionalSound)
                    source.SetPosition(screenPosition.X, screenPosition.Y, 0f);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        public void SetListenerPosition(Vector2 listenerPos)
        {
            Love.Audio.SetPosition(listenerPos.X, listenerPos.Y, 0f);
        }
    }
}

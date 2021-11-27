using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Config;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public class LoveAudioSystem : EntitySystem, IAudioSystem
    {
        private class PlayingSource
        {
            public Love.Source Source;

            public PlayingSource(Love.Source source)
            {
                Source = source;
            }
        }

        private List<PlayingSource> _playingSources = new();

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var source = Love.Audio.NewSource(prototype.ResolvePrototype().Path.ToString(), Love.SourceType.Static);

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
        public void Play(PrototypeId<SoundPrototype> prototype, IEntity entity, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            Play(prototype, entity.Coords, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var source = Love.Audio.NewSource(prototype.ResolvePrototype().Path.ToString(), Love.SourceType.Static);

            if (source.GetChannelCount() == 1)
            {
                var coords = GraphicsEx.Coords;
                coords.TileToScreen(coordinates.X, coordinates.Y, out var screenX, out var screenY);
                source.SetRelative(false);
                source.SetPosition(screenX, screenY, 0f);
                source.SetAttenuationDistances(100, 500);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }
    }
}

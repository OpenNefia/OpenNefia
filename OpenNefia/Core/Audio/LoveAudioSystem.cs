using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Config;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Game;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Audio
{
    public class LoveAudioSystem : EntitySystem, IAudioSystem
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private class PlayingSource
        {
            public Love.Source Source;

            public PlayingSource(Love.Source source)
            {
                Source = source;
            }
        }

        private List<PlayingSource> _playingSources = new();

        public override void Initialize()
        {
            SubscribeLocalEvent<FrameUpdateEventArgs>(OnFrameUpdate);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var fileData = _resourceCache.GetResource<LoveFileDataResource>(prototype.ResolvePrototype().Filepath);
            var source = Love.Audio.NewSource(fileData, Love.SourceType.Static);

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
            Play(prototype, entity.Spatial.Coords, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
            if (coordinates.Map != GameSession.ActiveMap)
                return;

            GameSession.Coords.TileToScreen(coordinates.Position, out var screenPosition);

            Play(prototype, screenPosition, audioParams);
        }

        /// <inheritdoc />
        public void Play(PrototypeId<SoundPrototype> prototype, Vector2i screenPosition, AudioParams? audioParams = null)
        {
            if (!ConfigVars.EnableSound)
                return;

            var source = Love.Audio.NewSource(prototype.ResolvePrototype().Filepath.ToString(), Love.SourceType.Static);

            if (source.GetChannelCount() == 1)
            {
                source.SetRelative(false);
                source.SetPosition(screenPosition.X, screenPosition.Y, 0f);
                source.SetAttenuationDistances(100, 500);
            }

            source.SetVolume(Math.Clamp(audioParams?.Volume ?? 1f, 0f, 1f));

            Love.Audio.Play(source);

            _playingSources.Add(new PlayingSource(source));
        }

        private void OnFrameUpdate(FrameUpdateEventArgs args)
        {
            var player = GameSession.Player;
            if (player == null)
                return;

            var coords = GameSession.Coords;
            coords.TileToScreen(player.Spatial.Coords.Position, out var listenerPos);
            listenerPos += coords.TileSize / 2;
            Love.Audio.SetPosition(listenerPos.X, listenerPos.Y, 0f);
        }
    }

    public struct FrameUpdateEventArgs
    {
        public float Dt;
    }
}

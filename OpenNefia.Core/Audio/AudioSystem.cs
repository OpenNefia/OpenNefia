using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Analyzers;

namespace OpenNefia.Core.Audio
{
    public sealed class AudioSystem : EntitySystem
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IAudioManager _audioManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, EntityPositionChangedEvent>(OnPlayerPositionChanged, nameof(OnPlayerPositionChanged));
        }

        // TODO: when implementing scrolling, run this every frame to account for sub-tile positioning.
        private void OnPlayerPositionChanged(EntityUid uid, PlayerComponent player, ref EntityPositionChangedEvent evt)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            var listenerPos = _coords.TileToScreen(spatial.MapPosition.Position) + _coords.TileSize / 2;
            _audioManager.SetListenerPosition(listenerPos);
        }
    }

    [EventArgsUsage(EventArgsTargets.ByRef)]
    public struct FrameUpdateEventArgs
    {
        public float Dt;
    }
}

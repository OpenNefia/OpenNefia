using OpenNefia.Core.Audio;

namespace OpenNefia.Core.GameObjects
{
    public class MapTransferSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, ExitMapEventArgs>(HandleExitMap);
        }

        private void HandleExitMap(EntityUid uid, PlayerComponent component, ExitMapEventArgs args)
        {
            Sounds.Play(SoundPrototypeOf.Exitmap1);
        }
    }
}

using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Content.GameObjects
{
    public class MapTransferSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, ExitMapEventArgs>(HandleExitMap, nameof(HandleExitMap));
        }

        private void HandleExitMap(EntityUid uid, PlayerComponent component, ExitMapEventArgs args)
        {
            Sounds.Play(SoundPrototypeOf.Exitmap1);
        }
    }
}

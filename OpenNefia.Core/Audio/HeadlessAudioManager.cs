using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public sealed class HeadlessAudioManager : IAudioManager
    {
        public void Initialize()
        {
        }

        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> prototype, EntityUid entityUid, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> prototype, Vector2i screenPosition, AudioParams? audioParams = null)
        {
        }

        public void SetListenerPosition(Vector2 listenerPos)
        {
        }
    }
}

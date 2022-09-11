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

        public void Shutdown()
        {
        }

        public void Play(PrototypeId<SoundPrototype> soundId, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> soundId, EntityUid entityUid, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> soundId, MapCoordinates coordinates, AudioParams? audioParams = null)
        {
        }
        
        public void Play(PrototypeId<SoundPrototype> soundId, EntityCoordinates coordinates, AudioParams? audioParams = null)
        {
        }

        public void Play(PrototypeId<SoundPrototype> soundId, Vector2i screenPosition, AudioParams? audioParams = null)
        {
        }

        public void SetListenerPosition(Vector2 listenerPos)
        {
        }
    }
}

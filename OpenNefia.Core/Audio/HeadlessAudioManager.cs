using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Timing;

namespace OpenNefia.Core.Audio
{
    public sealed class HeadlessAudioManager : IAudioManager
    {
        public void Initialize()
        {
        }

        public void FrameUpdate(FrameEventArgs frame)
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

        public void PlayLooping(PrototypeId<SoundPrototype> soundId, string tag, AudioParams? audioParams = null)
        {
        }

        public void StopLooping(string tag)
        {
        }

        public void StopAllLooping()
        {
        }
    }
}

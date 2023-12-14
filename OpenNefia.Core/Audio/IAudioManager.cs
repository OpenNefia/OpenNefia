using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Timing;

namespace OpenNefia.Core.Audio
{
    public interface IAudioManager
    {
        void Initialize();
        void FrameUpdate(FrameEventArgs frame);
        void Shutdown();

        /// <summary>
        /// Plays an audio file globally, without position.
        /// </summary>
        /// <param name="soundId">Prototype of the sound to play.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        void Play(PrototypeId<SoundPrototype> soundId, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file following an entity.
        /// </summary>
        /// <param name="soundId">Prototype of the sound to play.</param>
        /// <param name="entityUid">The UID of the entity "emitting" the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        void Play(PrototypeId<SoundPrototype> soundId, EntityUid entityUid, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file at a static position.
        /// </summary>
        /// <param name="soundId">Prototype of the sound to play.</param>
        /// <param name="coordinates">The coordinates at which to play the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        void Play(PrototypeId<SoundPrototype> soundId, MapCoordinates coordinates, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file at a static position.
        /// </summary>
        /// <param name="soundId">Prototype of the sound to play.</param>
        /// <param name="coordinates">The coordinates at which to play the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        void Play(PrototypeId<SoundPrototype> soundId, EntityCoordinates coordinates, AudioParams? audioParams = null);
        
        /// <summary>
        /// Plays an audio file at a static position.
        /// </summary>
        /// <param name="soundId">Prototype of the sound to play.</param>
        /// <param name="screenPosition">The coordinates at which to play the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        void Play(PrototypeId<SoundPrototype> soundId, Vector2i screenPosition, AudioParams? audioParams = null);

        void PlayLooping(PrototypeId<SoundPrototype> soundId, string tag, AudioParams? audioParams = null);
        void StopLooping(string tag);
        void StopAllLooping();

        /// <summary>
        /// Sets the listener position.
        /// </summary>
        void SetListenerPosition(Vector2 listenerPos);
    }
}
﻿using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    public interface IAudioSystem : IEntitySystem
    {
        /// <summary>
        /// Plays an audio file globally, without position.
        /// </summary>
        /// <param name="prototype">Prototype of the sound to play.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        public void Play(PrototypeId<SoundPrototype> prototype, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file following an entity.
        /// </summary>
        /// <param name="prototype">Prototype of the sound to play.</param>
        /// <param name="entity">The UID of the entity "emitting" the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        public void Play(PrototypeId<SoundPrototype> prototype, IEntity entity, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file at a static position.
        /// </summary>
        /// <param name="prototype">Prototype of the sound to play.</param>
        /// <param name="coordinates">The coordinates at which to play the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        public void Play(PrototypeId<SoundPrototype> prototype, MapCoordinates coordinates, AudioParams? audioParams = null);

        /// <summary>
        /// Plays an audio file at a static position.
        /// </summary>
        /// <param name="prototype">Prototype of the sound to play.</param>
        /// <param name="screenPosition">The coordinates at which to play the audio.</param>
        /// <param name="audioParams">Audio parameters to apply when playing the sound.</param>
        public void Play(PrototypeId<SoundPrototype> prototype, Vector2i screenPosition, AudioParams? audioParams = null);
    }
}

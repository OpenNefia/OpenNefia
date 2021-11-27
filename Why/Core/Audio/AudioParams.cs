using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Audio
{
    [DataDefinition]
    public struct AudioParams
    {
        /// <summary>
        ///     Base volume to play the audio at, in dB.
        /// </summary>
        public float Volume { get; set; }
    }
}

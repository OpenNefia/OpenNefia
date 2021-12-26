using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Audio
{
    /// <summary>
    /// A static proxy class for interfacing with the <see cref="IMusicManager"/>.
    /// </summary>
    [Obsolete]
    public static class Music
    {
        /// <summary>
        /// Plays a music file.
        /// </summary>
        /// <param name="prototype">Prototype of the music to play.</param>
        public static void Play(PrototypeId<MusicPrototype> prototype)
        {
            IoCManager.Resolve<IMusicManager>().Play(prototype);
        }

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public static void Stop()
        {
            IoCManager.Resolve<IMusicManager>().Stop();
        }
    }
}
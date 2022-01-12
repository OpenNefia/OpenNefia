using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Audio
{
    public interface IMusicManager
    {
        public bool IsPlaying { get; }

        public void Initialize();

        /// <summary>
        /// Plays a music file.
        /// </summary>
        /// <param name="prototype">Prototype of the music to play.</param>
        public void Play(PrototypeId<MusicPrototype> id);

        /// <summary>
        /// Stops playing music.
        /// </summary>
        public void Stop();
    }
}

using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Audio
{
    public sealed class HeadlessMusicManager : IMusicManager
    {
        public bool IsPlaying => false;

        public void Initialize()
        {
        }

        public void Play(PrototypeId<MusicPrototype> id)
        {
        }

        public void Stop()
        {
        }
    }
}

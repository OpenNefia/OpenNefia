using Melanchall.DryWetMidi.Multimedia;
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

        public void Shutdown()
        {
        }

        public IEnumerable<OutputDevice> GetMidiOutputDevices()
        {
            return Enumerable.Empty<OutputDevice>();
        }

        public void Play(PrototypeId<MusicPrototype> musicId, bool loop = true)
        {
        }

        public void Restart()
        {
        }

        public void Stop()
        {
        }
    }
}

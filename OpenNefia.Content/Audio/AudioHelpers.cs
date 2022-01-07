using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Audio
{
    public static class AudioHelpers
    {
        public static PrototypeId<SoundPrototype>? GetRandomFileFromSoundCollection(PrototypeId<SoundCollectionPrototype> soundID)
        {
            var soundCollection = IoCManager.Resolve<IPrototypeManager>()
                .Index(soundID);

            return IoCManager.Resolve<IRandom>().PickOrDefault(soundCollection.PickIDs);
        }
    }
}

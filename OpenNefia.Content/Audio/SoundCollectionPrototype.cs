using System.Collections.Generic;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Audio
{
    [Prototype("SoundCollection")]
    public sealed class SoundCollectionPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("pickIDs")]
        public List<PrototypeId<SoundPrototype>> PickIDs { get; } = new();
    }
}

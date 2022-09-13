using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Audio
{
    [Prototype("Sound")]
    public class SoundPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public ResourcePath Filepath { get; } = default!;
    }
}
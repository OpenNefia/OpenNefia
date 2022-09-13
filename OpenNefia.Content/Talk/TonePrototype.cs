using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Talk
{
    [Prototype("Elona.Tone")]
    public class TonePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }
}

using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    [Prototype("Dialog")]
    public class DialogPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;
    }
}

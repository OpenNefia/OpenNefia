using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    [DataDefinition]
    public sealed class PrototypeEventHandlerDef
    {
        [DataField("type", required: true)]
        public Type EventType { get; set; } = default!;

        [DataField("system", required: true)]
        public Type EntitySystemType { get; set; } = default!;

        [DataField("method", required: true)]
        public string MethodName { get; set; } = string.Empty;

        [DataField("priority")]
        public long Priority { get; set; } = EventPriorities.Default;
    }
}
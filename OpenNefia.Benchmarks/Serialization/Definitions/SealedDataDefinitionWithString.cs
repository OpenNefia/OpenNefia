using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Benchmarks.Serialization.Definitions
{
    [DataDefinition]
    public sealed class SealedDataDefinitionWithString
    {
        [DataField("string")]
        public string StringField { get; init; } = default!;
    }
}

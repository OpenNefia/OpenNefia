using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Benchmarks.Serialization.Definitions
{
    [DataDefinition]
    public class DataDefinitionWithString
    {
        [DataField("string")]
        public string StringField { get; init; } = default!;
    }
}

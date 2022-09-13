using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Guild
{
    [Prototype("Elona.Guild")]
    public class GuildPrototype : IPrototype
    {
        /// <inheritdoc />
        [IdDataField]
        public string ID { get; private set; } = default!;
    }
}

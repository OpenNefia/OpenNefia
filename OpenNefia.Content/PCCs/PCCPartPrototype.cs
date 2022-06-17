using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.PCCs
{
    [Prototype("Elona.PCCPart")]
    public class MapTilesetPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField("pccPartType", required:true)]
        public PCCPartType PCCPartType { get; }

        [DataField(required: true)]
        public ResourcePath ImagePath { get; } = default!;
    }
}
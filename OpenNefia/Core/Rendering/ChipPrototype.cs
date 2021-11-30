using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [Prototype("Chip", -1)]
    public class ChipPrototype : IPrototype, IAtlasRegionProvider
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public TileSpecifier Image { get; } = null!;

        [DataField]
        public string Group { get; } = string.Empty;

        public IEnumerable<AtlasRegion> GetAtlasRegions()
        {
            yield return new(AtlasNames.Chip, $"{ID}:Default", Image);
        }
    }
}

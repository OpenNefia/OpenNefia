using OpenNefia.Core.Maths;
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

        /// <summary>
        /// Offset to render this chip at, in pixels.
        /// </summary>
        [DataField]
        public Vector2i Offset { get; } = Vector2i.Zero;

        /// <summary>
        /// Offset to apply when rendering this chip as part of a stack, in pixels.
        /// </summary>
        [DataField]
        public int StackYOffset { get; } = 0;

        public IEnumerable<AtlasRegion> GetAtlasRegions()
        {
            yield return new(AtlasNames.Chip, $"{ID}:Default", Image);
        }
    }
}

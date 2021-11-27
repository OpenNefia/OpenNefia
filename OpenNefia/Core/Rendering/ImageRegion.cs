using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class ImageRegion
    {
        [DataField(required: true)]
        public int X = 0;

        [DataField(required: true)]
        public int Y = 0;

        [DataField(required: true)]
        public int Width = 0;

        [DataField(required: true)]
        public int Height = 0;

        public Color? KeyColor { get; set; } = null;
    }
}

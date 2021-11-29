using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class FontSpec
    {
        public FontSpec(int size, int smallSize)
        {
            Size = size;
            SmallSize = smallSize;
        }

        [DataField(required: true)]
        public int Size { get; }

        [DataField(required: true)]
        public int SmallSize { get; }

        [DataField]
        public FontStyle Style { get; } = FontStyle.None;

        private Love.Font? _font = null;

        public Love.Font LoveFont => _font ??= IoCManager.Resolve<IFontManager>().GetFont(this);
    }
}

using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class FontSpec
    {
        public FontSpec(int size = 14, int smallSize = 12)
        {
            Size = size;
            SmallSize = smallSize;
        }

        public FontSpec()
        {
        }

        [DataField(required: true)]
        public int Size { get; } = 14;

        [DataField(required: true)]
        public int SmallSize { get; } = 12;

        [DataField]
        public FontStyle Style { get; } = FontStyle.None;

        private Love.Font? _font = null;

        public Love.Font LoveFont => _font ??= IoCManager.Resolve<IFontManager>().GetFont(this);
    }
}

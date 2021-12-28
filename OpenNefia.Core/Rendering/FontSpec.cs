using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class FontSpec
    {
        public FontSpec(int size = 14, int smallSize = 12, Maths.Color? color = null, Maths.Color? bgColor = null, FontStyle style = FontStyle.None)
        {
            Size = size;
            SmallSize = smallSize;
            if (color != null)
                Color = color.Value;
            if (bgColor != null)
                BgColor = bgColor.Value;
            Style = style;
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

        [DataField]
        public Maths.Color Color { get; } = Color.Black;

        [DataField]
        public Maths.Color BgColor { get; } = Color.Black;

        private Love.Font? _font = null;

        public Love.Font LoveFont => _font ??= IoCManager.Resolve<IFontManager>().GetFont(this);
    }
}

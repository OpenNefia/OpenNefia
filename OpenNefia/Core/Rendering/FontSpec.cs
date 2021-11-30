using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class FontSpec
    {
        public FontSpec(int size = 14, int smallSize = 12, Maths.Color? color = null, Maths.Color? bgColor = null)
        {
            Size = size;
            SmallSize = smallSize;
            if (color != null)
                Color = color.Value;
            if (bgColor != null)
                BgColor = bgColor.Value;
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
        public Maths.Color Color { get; } = UiColors.TextBlack;

        [DataField]
        public Maths.Color BgColor { get; } = UiColors.TextWhite;

        private Love.Font? _font = null;

        public Love.Font LoveFont => _font ??= IoCManager.Resolve<IFontManager>().GetFont(this);
    }
}

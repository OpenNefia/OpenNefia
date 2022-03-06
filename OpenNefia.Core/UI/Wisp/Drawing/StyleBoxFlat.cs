using System;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UI.Wisp.Drawing
{
    public sealed class StyleBoxFlat : StyleBox
    {
        public Color BackgroundColor { get; set; }
        public Color BorderColor { get; set; }
        public Thickness BorderThickness { get; set; }

        protected override void DoDraw(UIBox2 box, float uiScale)
        {
            Love.Graphics.SetColor(BorderColor);

            var (btl, btt, btr, btb) = BorderThickness;
            if (btl > 0)
                GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, new UIBox2(box.Left, box.Top, box.Left + btl, box.Bottom));

            if (btt > 0)
                GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, new UIBox2(box.Left, box.Top, box.Right, box.Top + btt));

            if (btr > 0)
                GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, new UIBox2(box.Right - btr, box.Top, box.Right, box.Bottom));

            if (btb > 0)
                GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, new UIBox2(box.Left, box.Bottom - btb, box.Right, box.Bottom));

            Love.Graphics.SetColor(BackgroundColor);
            GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, BorderThickness.Deflate(box));
        }

        public StyleBoxFlat()
        {
        }

        public StyleBoxFlat(Color backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public StyleBoxFlat(StyleBoxFlat other)
            : base(other)
        {
            BackgroundColor = other.BackgroundColor;
            BorderColor = other.BorderColor;
            BorderThickness = other.BorderThickness;
        }

        protected override float GetDefaultContentMargin(Margin margin)
        {
            return margin switch
            {
                Margin.Top => BorderThickness.Top,
                Margin.Bottom => BorderThickness.Bottom,
                Margin.Right => BorderThickness.Right,
                Margin.Left => BorderThickness.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(margin), margin, null)
            };
        }
    }
}

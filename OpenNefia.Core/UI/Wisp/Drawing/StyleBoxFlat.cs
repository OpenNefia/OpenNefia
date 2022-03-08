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

        protected override void DoDraw(UIBox2 pixelBox)
        {
            Love.Graphics.SetColor(BorderColor);

            var (btl, btt, btr, btb) = BorderThickness;
            if (btl > 0)
                GraphicsEx.Rectangle(Love.DrawMode.Fill, new UIBox2(pixelBox.Left, pixelBox.Top, pixelBox.Left + btl, pixelBox.Bottom));

            if (btt > 0)
                GraphicsEx.Rectangle(Love.DrawMode.Fill, new UIBox2(pixelBox.Left, pixelBox.Top, pixelBox.Right, pixelBox.Top + btt));

            if (btr > 0)
                GraphicsEx.Rectangle(Love.DrawMode.Fill, new UIBox2(pixelBox.Right - btr, pixelBox.Top, pixelBox.Right, pixelBox.Bottom));

            if (btb > 0)
                GraphicsEx.Rectangle(Love.DrawMode.Fill, new UIBox2(pixelBox.Left, pixelBox.Bottom - btb, pixelBox.Right, pixelBox.Bottom));

            Love.Graphics.SetColor(BackgroundColor);
            GraphicsEx.Rectangle(Love.DrawMode.Fill, BorderThickness.Deflate(pixelBox));
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

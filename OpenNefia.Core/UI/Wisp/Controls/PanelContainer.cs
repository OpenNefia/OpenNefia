using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp.Drawing;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class PanelContainer : Container
    {
        public const string StylePropertyPanel = "panel";

        public StyleBox? PanelOverride { get; set; }

        public override void Draw()
        {
            var style = _getStyleBox();
            style?.Draw(GlobalPixelRect);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var styleSize = (_getStyleBox()?.MinimumSize ?? Vector2.Zero) / UIScale;
            var measureSize = Vector2.ComponentMax(availableSize - styleSize, Vector2.Zero);
            var childSize = Vector2.Zero;
            foreach (var child in WispChildren)
            {
                child.Measure(measureSize);
                childSize = Vector2.ComponentMax(childSize, child.DesiredSize);
            }

            return styleSize + childSize;
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            var pixelSize = finalSize * UIScale;
            var ourSize = UIBox2.FromDimensions(Vector2.Zero, pixelSize);
            var contentBox = _getStyleBox()?.GetContentBox(ourSize) ?? ourSize;

            foreach (var child in WispChildren)
            {
                child.ArrangePixel((UIBox2i)contentBox);
            }

            return finalSize;
        }

        [System.Diagnostics.Contracts.Pure]
        private StyleBox? _getStyleBox()
        {
            if (PanelOverride != null)
            {
                return PanelOverride;
            }

            TryGetStyleProperty<StyleBox>(StylePropertyPanel, out var box);
            return box;
        }
    }
}

using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using static OpenNefia.Core.Rendering.AssetDrawable;

namespace OpenNefia.Core.UI.Element
{
    public class UiWindowBacking : BaseDrawable
    {
        public enum WindowBackingType
        {
            Normal,
            Shadow
        }

        private IAssetDrawable? AssetWindow;
        private Love.SpriteBatch? Batch;
        private WindowBackingType Type;

        public UiWindowBacking(WindowBackingType type = WindowBackingType.Normal)
        {
            this.Type = type;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            var x = 0;
            var y = 0;

            var x_inner = width + x - width % 8 - 64;
            var y_inner = height + y - height % 8 - 64;

            y_inner = Math.Max(y_inner, y + 14);

            var parts = new List<AssetBatchPart>();

            if (this.Type != WindowBackingType.Shadow)
            {
                parts.Add(new AssetBatchPart("top_left", x, y));
            }
            parts.Add(new AssetBatchPart("top_right", x_inner, y));
            parts.Add(new AssetBatchPart("bottom_left", x, y_inner));
            parts.Add(new AssetBatchPart("bottom_right", x_inner, y_inner));

            for (int dx = 8; dx < width / 8 - 8; dx++)
            {
                var tile = Math.Abs((dx - 8) % 18);
                if (this.Type != WindowBackingType.Shadow)
                {
                    parts.Add(new AssetBatchPart($"top_mid_{tile}", dx * 8 + x, y));
                }
                parts.Add(new AssetBatchPart($"bottom_mid_{tile}", dx * 8 + x, y_inner));
            }

            for (int dy = 0; dy < height / 8 - 13; dy++)
            {
                var tile_y = dy % 12;
                if (this.Type != WindowBackingType.Shadow)
                {
                    parts.Add(new AssetBatchPart($"mid_left_{tile_y}", x, dy * 8 + y + 48));

                    for (int dx = 0; dx < width / 8 - 14; dx++)
                    {
                        var tile_x = Math.Abs((dx - 8) % 18);
                        parts.Add(new AssetBatchPart($"mid_mid_{tile_y}_{tile_x}", dx * 8 + x + 56, dy * 8 + y + 48));
                    }
                }
                parts.Add(new AssetBatchPart($"mid_right_{tile_y}", x_inner, dy * 8 + y + 48));
            }

            this.AssetWindow = Assets.GetSized(AssetPrototypeOf.Window, this.Size);
            this.Batch = this.AssetWindow.MakeBatch(parts);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            GraphicsEx.DrawSpriteBatch(this.Batch!, this.X, this.Y);
        }

        public override void Dispose()
        {
            this.Batch?.Dispose();
        }
    }
}

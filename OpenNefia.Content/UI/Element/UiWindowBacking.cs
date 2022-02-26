using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Core.Rendering.AssetInstance;

namespace OpenNefia.Content.UI.Element
{
    public class UiWindowBacking : UiElement
    {
        public enum WindowBackingType
        {
            Normal,
            Shadow
        }

        private PrototypeId<AssetPrototype> ProtoId;
        private IAssetInstance? AssetWindow;
        private Love.SpriteBatch? Batch;
        private WindowBackingType Type;

        public UiWindowBacking(PrototypeId<AssetPrototype> protoId, WindowBackingType type = WindowBackingType.Normal)
        {
            ProtoId = protoId;
            Type = type;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            var x_inner = PixelWidth + 0 - PixelWidth % 8 - 64;
            var y_inner = PixelHeight + 0 - PixelHeight % 8 - 64;

            y_inner = Math.Max(y_inner, 0 + 14);

            var parts = new List<AssetBatchPart>();

            if (Type != WindowBackingType.Shadow)
            {
                parts.Add(new AssetBatchPart("top_left", 0, 0));
            }
            parts.Add(new AssetBatchPart("top_right", x_inner, 0));
            parts.Add(new AssetBatchPart("bottom_left", 0, y_inner));
            parts.Add(new AssetBatchPart("bottom_right", x_inner, y_inner));

            for (int dx = 8; dx < PixelWidth / 8 - 8; dx++)
            {
                var tile = Math.Abs((dx - 8) % 18);
                if (Type != WindowBackingType.Shadow)
                {
                    parts.Add(new AssetBatchPart($"top_mid_{tile}", dx * 8 + 0, 0));
                }
                parts.Add(new AssetBatchPart($"bottom_mid_{tile}", dx * 8 + 0, y_inner));
            }

            for (int dy = 0; dy < PixelHeight / 8 - 13; dy++)
            {
                var tile_y = dy % 12;
                if (Type != WindowBackingType.Shadow)
                {
                    parts.Add(new AssetBatchPart($"mid_left_{tile_y}", 0, dy * 8 + 0 + 48));

                    for (int dx = 0; dx < PixelWidth / 8 - 14; dx++)
                    {
                        var tile_x = Math.Abs((dx - 8) % 18);
                        parts.Add(new AssetBatchPart($"mid_mid_{tile_y}_{tile_x}", dx * 8 + 0 + 56, dy * 8 + 0 + 48));
                    }
                }
                parts.Add(new AssetBatchPart($"mid_right_{tile_y}", x_inner, dy * 8 + 0 + 48));
            }

            AssetWindow = Assets.GetSized(ProtoId, PixelSize);
            Batch = AssetWindow.MakeBatch(parts);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            GraphicsEx.DrawSpriteBatchS(UIScale, Batch!, X, Y, Width, Height);
        }

        public override void Dispose()
        {
            Batch?.Dispose();
        }
    }
}

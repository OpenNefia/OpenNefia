using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using static OpenNefia.Core.Rendering.AssetInstance;

namespace OpenNefia.Content.UI.Element
{
    public class UiHudBar : UiElement
    {
        private IAssetInstance HudBarAsset = default!;
        private Love.SpriteBatch Batch = default!;

        public const int HudBarWidth = 192;
        public const int HudBarHeight = 24;

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            HudBarAsset = Assets.GetSized(Protos.Asset.HudBar, PixelSize);

            var parts = new List<AssetBatchPart>();
            for (int x = 0; x < PixelWidth / HudBarWidth; x++)
            {
                parts.Add(new AssetBatchPart("bar",(x * HudBarWidth), 0));
            }

            Batch?.Dispose();
            Batch = HudBarAsset.MakeBatch(parts);
        }

        public override void Draw()
        {
            GraphicsS.DrawS(UIScale, Batch, X, Y);
        }

        public override void Update(float dt)
        {
        }
    }
}

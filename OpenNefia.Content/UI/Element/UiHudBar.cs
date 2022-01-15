using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Rendering.AssetInstance;

namespace OpenNefia.Content.UI.Element
{
    public class UiHudBar : BaseDrawable
    {
        public const int HudBarWidth = 192;
        public const int HudBarHeight = 24;

        private IAssetInstance HudBarAsset = default!;
        private Love.SpriteBatch Batch = default!;

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            var parts = new List<AssetBatchPart>();
            for (int x = 0; x < Width / HudBarWidth; x++)
            {
                parts.Add(new AssetBatchPart("bar", X + (x * HudBarWidth), Y));
            }

            HudBarAsset = Assets.GetSized(Protos.Asset.HudBar, PixelSize);
            Batch = HudBarAsset.MakeBatch(parts);
        }

        public override void Draw()
        {
            Love.Graphics.Draw(Batch, X, Y);
        }

        public override void Update(float dt)
        {
            
        }
    }
}

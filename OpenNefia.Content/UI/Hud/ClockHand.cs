using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Rendering.AssetInstance;
using Love;

namespace OpenNefia.Content.UI.Hud
{
    public class ClockHand : BaseDrawable
    {
        // Angle in radians
        private float Angle;
        private SpriteBatch Batch;
        public ClockHand()
        {
            var asset = Assets.Get(Protos.Asset.ClockHand);
            var parts = new List<AssetBatchPart>();
            parts.Add(new AssetBatchPart("0", 0, 0));
            Batch = asset.MakeBatch(parts);
        }

        public void SetHour(int hour)
        {
            Angle = (float)Core.Maths.Angle.FromDegrees(hour * 30).Theta;
        }

        public override void Draw()
        {
            Graphics.Draw(Batch, X, Y, Angle, 1, 1, 24, 21.5f);
        }

        public override void Update(float dt)
        {
        }
    }
}

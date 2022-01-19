using Love;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Rendering.AssetInstance;
using OpenNefia.Content.World;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Hud
{
    public class ClockHand : BaseDrawable
    {
        // Angle in radians
        private float Angle;
        private SpriteBatch Batch;
        public ClockHand()
        {
            var asset = Assets.Get(Protos.Asset.ClockHand);
            var parts = new List<AssetBatchPart>{ new("0", 0, 0) };
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

    public class HudClockWidget : BaseHudWidget
    {
        [Dependency] private readonly IWorldSystem _world = default!;

        private IAssetInstance ClockAsset = default!;
        private ClockHand ClockHand = default!;

        public override void Initialize()
        {
            //= EntitySystem.Get<WorldSystem>();
            base.Initialize();
            ClockAsset = Assets.Get(Protos.Asset.Clock);
            ClockHand = new ClockHand();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            ClockHand.SetPosition(x + 62, y + 48);
        }

        public override void Draw()
        {
            ClockAsset.Draw(X, Y);
            ClockHand.Draw();
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            ClockHand.SetHour(_world.State.GameDate.Hour);
        }
    }
}

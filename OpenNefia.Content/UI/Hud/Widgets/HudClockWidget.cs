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
using OpenNefia.Core.UI;

namespace OpenNefia.Content.Hud
{
    public class ClockHand : UiElement
    {
        [Child] private AssetDrawable HandAsset;

        public ClockHand()
        {
            HandAsset = new AssetDrawable(Protos.Asset.ClockHand, centered: true, regionId: "0", originOffset: new(0, -2.5f));
        }

        public void SetHour(int hour)
        {
            HandAsset.Rotation = (float)Core.Maths.Angle.FromDegrees(hour * 30).Theta;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            HandAsset.SetPosition(X, Y);
        }

        public override void Draw()
        {
            HandAsset.Draw();
        }

        public override void Update(float dt)
        {
        }
    }

    public class HudClockWidget : BaseHudWidget
    {
        [Dependency] private readonly IWorldSystem _world = default!;

        private IAssetInstance ClockAsset = default!;
        [Child] private ClockHand ClockHand = new();

        public override void Initialize()
        {
            base.Initialize();
            ClockAsset = Assets.Get(Protos.Asset.Clock);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            ClockHand.SetPosition(X + 62, Y + 48);
        }

        public override void Draw()
        {
            ClockAsset.Draw(UIScale, X, Y);
            ClockHand.Draw();
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            ClockHand.SetHour(_world.State.GameDate.Hour);
        }
    }
}

using OpenNefia.Content.Hud;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.World;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hud
{
    public class HudDateWidget : BaseHudWidget
    {
        [Dependency] private readonly IWorldSystem _world = default!;

        private IAssetInstance DateFrame = default!; // FIXME: #94
        [Child] private UiText DateText = new UiText();

        public override void Initialize()
        {
            base.Initialize();
            DateFrame = Assets.Get(Protos.Asset.DateLabelFrame);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            DateText.SetPosition(X + 40, Y + 9);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            var date = _world.State.GameDate;
            DateText.Text = $"{date.Year}/{date.Month}/{date.Day}";
        }

        public override void Draw()
        {
            base.Draw();
            DateFrame.Draw(UIScale, X, Y);
            DateText.Draw();
        }
    }
}

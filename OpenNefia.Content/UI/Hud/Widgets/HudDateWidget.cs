using OpenNefia.Content.Hud;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.World;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
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

        private IAssetInstance DateFrame = default!;
        private IUiText DateText = default!;

        public override void Initialize()
        {
            base.Initialize();
            DateFrame = Assets.Get(Protos.Asset.DateLabelFrame);
            DateText = new UiText();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            DateText.SetPosition(x + 40, y + 9);
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
            DateFrame.Draw(X, Y);
            DateText.Draw();
        }
    }
}

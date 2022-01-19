using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.Hud
{
    public class HudAreaNameWidget : BaseHudWidget
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private IDisplayNameSystem _nameSystem = default!;

        private IAssetInstance MapNameIcon;
        private IUiText UiText;

        public HudAreaNameWidget()
        {
            MapNameIcon = Assets.Get(Protos.Asset.MapNameIcon);
            UiText = new UiText(UiFonts.HUDSkillText);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_mapManager.ActiveMap?.MapEntityUid != null)
                UiText.Text = _nameSystem.GetDisplayName(_mapManager.ActiveMap.MapEntityUid);
            else
                UiText.Text = string.Empty;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(x + 20, y + 2);
        }

        public override void Draw()
        {
            base.Draw();
            MapNameIcon.Draw(X, Y);
            UiText.Draw();
        }
    }
}

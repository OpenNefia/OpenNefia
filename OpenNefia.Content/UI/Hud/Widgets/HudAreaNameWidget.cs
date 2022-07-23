using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.DisplayName;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.Hud
{
    public class HudAreaNameWidget : BaseHudWidget
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private IDisplayNameSystem _nameSystem = default!;

        private IAssetInstance MapNameIcon;
        [Child] private UiText UiText;

        public HudAreaNameWidget()
        {
            MapNameIcon = Assets.Get(Protos.Asset.MapNameIcon);
            UiText = new UiText(UiFonts.HUDSkillText);
        }

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_mapManager.ActiveMap?.MapEntityUid != null)
                UiText.Text = _nameSystem.GetDisplayName(_mapManager.ActiveMap.MapEntityUid);
            else
                UiText.Text = string.Empty;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + 20, Y + 2);
        }

        public override void Draw()
        {
            base.Draw();
            MapNameIcon.Draw(UIScale, X, Y);
            UiText.Draw();
        }
    }
}

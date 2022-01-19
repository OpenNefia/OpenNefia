using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Hud
{
    public abstract class HudBarBaseWidget : BaseHudWidget
    {

        protected abstract string BarText { get; set; }
        protected abstract IAssetInstance BarAsset { get; set; }
        protected abstract float BarRatio { get; set; }
        protected virtual Vector2i BarSize { get; } = new(83, 7);
        protected virtual Vector2 BarOffset { get; } = new(17, 5);

        private UiHelpers.UiBarDrawableState BarState = default!;
        private IAssetInstance BarBGAsset;
        private IUiText UiText;

        public HudBarBaseWidget()
        {
            UiText = new UiTextOutlined(UiFonts.HUDBarText);
            BarBGAsset = Assets.Get(Protos.Asset.HpBarFrame);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(x + 20, y - 7);
        }

        protected void SetState()
        {
            UiText.Text = BarText;
            BarState = new UiHelpers.UiBarDrawableState(BarAsset, Math.Clamp(BarRatio, 0f, 1f), new());
        }

        public override void Draw()
        {
            base.Draw();
            BarBGAsset.Draw(X, Y);
            UiHelpers.DrawPercentageBar(BarState, new Vector2(X, Y) + BarOffset, BarState.HPRatio * BarSize.X, new());
            UiText.Draw();
        }
    }

    public class HudHPBarWidget : HudBarBaseWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override string BarText { get; set; } = default!;
        protected override IAssetInstance BarAsset { get; set; } = default!;
        protected override float BarRatio { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            BarAsset = Assets.Get(Protos.Asset.HudHpBar);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                BarText = $"{skills.HP}({skills.MaxHP})";
                BarRatio = (float)skills.HP / skills.MaxHP;
            }
            SetState();
        }
    }

    public class HudMPBarWidget : HudBarBaseWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override string BarText { get; set; } = default!;
        protected override IAssetInstance BarAsset { get; set; } = default!;
        protected override float BarRatio { get; set; }

        public override void Initialize()
        {
            base.Initialize();
            BarAsset = Assets.Get(Protos.Asset.HudMpBar);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                BarText = $"{skills.MP}({skills.MaxMP})";
                BarRatio = (float)skills.MP / skills.MaxMP;
            }
            SetState();
        }
    }
}

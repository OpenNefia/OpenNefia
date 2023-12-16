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
using OpenNefia.Core.UI;
using OpenNefia.Content.Mount;

namespace OpenNefia.Content.Hud
{
    public abstract class HudBarBaseWidget : BaseHudWidget
    {
        protected abstract string BarText { get; set; }
        protected abstract IAssetInstance BarAsset { get; set; }
        protected abstract float BarRatio { get; set; }
        protected virtual Vector2 BarSize { get; } = new(83, 7);
        protected virtual Vector2 BarOffset { get; } = new(17, 5);

        private UiHelpers.UiBarDrawableState BarState = default!;
        private IAssetInstance BarBGAsset;
        [Child] private UiText UiText;

        public HudBarBaseWidget()
        {
            UiText = new UiTextOutlined(UiFonts.HUDBarText);
            BarBGAsset = Assets.Get(Protos.Asset.HpBarFrame);

            MinSize = ExactSize = BarSize;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + 20, Y - 7);
        }

        protected void RefreshBarState()
        {
            UiText.Text = BarText;
            BarState = new UiHelpers.UiBarDrawableState(BarAsset, Math.Clamp(BarRatio, 0f, 1f), Vector2.Zero);
        }

        public override void Draw()
        {
            base.Draw();
            BarBGAsset.Draw(UIScale, X, Y);
            UiHelpers.DrawPercentageBar(UIScale, BarState, Position + BarOffset, BarState.HPRatio * BarSize.X);
            UiText.Draw();
        }
    }

    public sealed class HudHPBarWidget : HudBarBaseWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override string BarText { get; set; } = string.Empty;
        protected override IAssetInstance BarAsset { get; set; } = default!;
        protected override float BarRatio { get; set; }

        public HudHPBarWidget()
        {
            BarAsset = Assets.Get(Protos.Asset.HudHpBar);
        }

        public override void RefreshWidget()
        {
            // >>>>>>>> elona122/shade2/screen.hsp:149 	if cHP(pc)>0{ ...
            base.RefreshWidget();
            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                BarText = $"{skills.HP}({skills.MaxHP})";
                BarRatio = (float)skills.HP / skills.MaxHP;
            }
            RefreshBarState();
            // <<<<<<<< elona122/shade2/screen.hsp:152 		} ...
        }
    }

    public sealed class HudMountHPBarWidget : HudBarBaseWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override string BarText { get; set; } = string.Empty;
        protected override IAssetInstance BarAsset { get; set; } = default!;
        protected override float BarRatio { get; set; }

        public HudMountHPBarWidget()
        {
            BarAsset = Assets.Get(Protos.Asset.HudHpBar);
        }

        public override void RefreshWidget()
        {
            // >>>>>>>> elona122/shade2/screen.hsp:159 	if gRider!0 : if cExist(gRider)=cAlive{ ...
            base.RefreshWidget();

            if (!_entMan.TryGetComponent<MountRiderComponent>(GameSession.Player, out var rider)
                || !_entMan.IsAlive(rider.Mount))
            {
                Visible = false;
                RefreshBarState();
                return;
            }

            Visible = true;

            if (_entMan.TryGetComponent<SkillsComponent>(rider.Mount.Value, out var skills))
            {
                BarText = $"{skills.HP}({skills.MaxHP})";
                BarRatio = (float)skills.HP / skills.MaxHP;
            }
            RefreshBarState();
            // <<<<<<<< elona122/shade2/screen.hsp:163 		} ...
        }
    }

    public sealed class HudMPBarWidget : HudBarBaseWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override string BarText { get; set; } = string.Empty;
        protected override IAssetInstance BarAsset { get; set; }
        protected override float BarRatio { get; set; }

        public HudMPBarWidget()
        {
            BarAsset = Assets.Get(Protos.Asset.HudMpBar);
        }

        public override void RefreshWidget()
        {
            // >>>>>>>> elona122/shade2/screen.hsp:155 	if cMP(pc)>0{ ...
            base.RefreshWidget();
            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                BarText = $"{skills.MP}({skills.MaxMP})";
                BarRatio = (float)skills.MP / skills.MaxMP;
            }
            RefreshBarState();
            // <<<<<<<< elona122/shade2/screen.hsp:158 		} ...
        }
    }
}

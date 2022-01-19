using OpenNefia.Content.Currency;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Levels;

namespace OpenNefia.Content.Hud
{
    public abstract class HudCurrencyWidget : BaseHudWidget
    {
        protected abstract IAssetInstance Icon { get; set; }
        protected abstract string Text { get; set; }

        private IUiText UiText = default!;

        public override void Initialize()
        {
            base.Initialize();
            UiText = new UiTextOutlined(UiFonts.HUDInfoText);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            UiText.Text = Text ?? string.Empty;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(x + 30, y + 3);
        }

        public override void Draw()
        {
            base.Draw();
            Icon.Draw(X, Y);
            UiText.Draw();
        }
    }

    public class HudGoldWidget : HudCurrencyWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override IAssetInstance Icon { get; set; } = default!;
        protected override string Text { get; set; } = default!;

        public override void Initialize()
        {
            base.Initialize();
            Icon = Assets.Get(Protos.Asset.GoldCoin);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<WalletComponent>(GameSession.Player, out var wallet))
            {
                Text = $"{wallet.Gold} {Loc.GetString("Elona.Hud.Info.Gold")}";
            }
        }
    }

    public class HudPlatinumWidget : HudCurrencyWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override IAssetInstance Icon { get; set; } = default!;
        protected override string Text { get; set; } = default!;

        public override void Initialize()
        {
            base.Initialize();
            Icon = Assets.Get(Protos.Asset.PlatinumCoin);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<WalletComponent>(GameSession.Player, out var wallet))
            {
                Text = $"{wallet.Platinum} {Loc.GetString("Elona.Hud.Info.Platinum")}";
            }
        }
    }

    public class HudExpWidget : HudCurrencyWidget
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        protected override IAssetInstance Icon { get; set; } = default!;
        protected override string Text { get; set; } = default!;

        public override void Initialize()
        {
            base.Initialize();
            Icon = Assets.Get(Protos.Asset.CharacterLevelIcon);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<LevelComponent>(GameSession.Player, out var level))
            {
                Text = $"{Loc.GetString("Elona.Hud.Info.Level")}{level.Level}/{level.ExperienceToNext}";
            }
        }
    }
}

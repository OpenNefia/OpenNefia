using OpenNefia.Content.Currency;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Levels;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.Hud
{
    public abstract class HudCurrencyWidget : BaseHudWidget
    {
        protected abstract IAssetInstance Icon { get; set; }
        protected abstract string Text { get; set; }

        [Child] private UiText UiText = new UiTextOutlined(UiFonts.HUDInfoText);

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            UiText.Text = Text ?? string.Empty;
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(x + 30, y + 3);
        }

        public override void Draw()
        {
            base.Draw();
            Icon.Draw(UIScale, X, Y);
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

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_entMan.TryGetComponent<MoneyComponent>(GameSession.Player, out var wallet))
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

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_entMan.TryGetComponent<MoneyComponent>(GameSession.Player, out var wallet))
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

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_entMan.TryGetComponent<LevelComponent>(GameSession.Player, out var level))
            {
                Text = $"{Loc.GetString("Elona.Hud.Info.Level")}{level.Level}/{level.Experience}";
            }
        }
    }
}

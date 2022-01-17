using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Stats;
using OpenNefia.Content.DisplayName;

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : UiLayer, IHudLayer
    {
        private enum HudSkillIconType
        {
            Str = 0,
            Con = 1,
            Dex = 2,
            Per = 3,
            Lea = 4,
            Wil = 5,
            Mag = 6,
            Cha = 7,
            Spd = 8,
            DvPv = 9
        }

        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        // These systems can't be gotten through the IoC at the time of the the first instantiation
        private IDisplayNameSystem _nameSystem = default!;
        private IWorldSystem _world = default!;

        public IHudMessageWindow MessageWindow { get; private set; } = default!;

        public Vector2i HudScreenOffset { get; } = new(0, MinimapHeight);

        private UiFpsCounter FpsCounter;
        private BaseDrawable MessageBoxBacking = default!;
        private BaseDrawable BacklogBacking = default!;
        private BaseDrawable HudBar = default!;

        private UiContainer MessageBoxContainer = default!;
        public UiContainer BacklogContainer = default!;

        private IAssetInstance MiniMapAsset = default!;
        private IAssetInstance ClockAsset = default!;
        private IAssetInstance DateFrame = default!;
        private IAssetInstance BarAsset = default!;
        private IAssetInstance HpBarAsset = default!;
        private IAssetInstance MpBarAsset = default!;
        private IAssetInstance ExpIconAsset = default!;
        private IAssetInstance GoldIconAsset = default!;
        private IAssetInstance PlatIconAsset = default!;
        private IAssetInstance SkillIcons = default!;
        private IAssetInstance MapNameIcon = default!;
        private UiHelpers.DrawEntry HpBarEntry = default!;
        private UiHelpers.DrawEntry MpBarEntry = default!;
        private IUiText DateText = default!;
        private IUiText HealthText = default!;
        private IUiText ManaText = default!;
        private IUiText ExpText = default!;
        private IUiText GoldText = default!;
        private IUiText PlatText = default!;
        private IUiText MapNameText = default!;
        private ClockHand ClockHand = default!;
        private Minimap Minimap = default!;
        private UiHorizontalContainer HudSkillContainer;

        private bool ShowingBacklog;
        private Dictionary<HudSkillIconType, IUiText> SkillTexts = new();

        private const int MinimapWidth = 122;
        private const int MinimapHeight = 88;
        // TODO: replace when containing components exist
        private const string GoldPlaceholder = "0";
        private const string PlatPlaceholder = "0";

        public const int HudZOrder = 200000000;

        public HudLayer()
        {
            IoCManager.InjectDependencies(this);
            FpsCounter = new UiFpsCounter();
            HudSkillContainer = new UiHorizontalContainer { XMinWidth = 47 };
        }

        public void Initialize()
        {
            _world = EntitySystem.Get<WorldSystem>();
            _nameSystem = EntitySystem.Get<DisplayNameSystem>();
            DateText = new UiText();

            CanKeyboardFocus = true;
            MessageBoxBacking = new UiMessageWindowBacking();
            BacklogBacking = new UiMessageWindowBacking(UiMessageWindowBacking.MessageBackingType.Expanded);
            HudBar = new UiHudBar();
            MiniMapAsset = Assets.Get(Protos.Asset.HudMinimap);
            Minimap = new Minimap();
            ClockAsset = Assets.Get(Protos.Asset.Clock);
            ClockHand = new ClockHand();
            DateFrame = Assets.Get(Protos.Asset.DateLabelFrame);

            BarAsset = Assets.Get(Protos.Asset.HpBarFrame);
            HpBarAsset = Assets.Get(Protos.Asset.HudHpBar);
            MpBarAsset = Assets.Get(Protos.Asset.HudMpBar);

            HealthText = new UiTextOutlined(UiFonts.HUDBarText);
            ManaText = new UiTextOutlined(UiFonts.HUDBarText);

            ExpText = new UiTextOutlined(UiFonts.HUDInfoText);
            GoldText = new UiTextOutlined(UiFonts.HUDInfoText);
            PlatText = new UiTextOutlined(UiFonts.HUDInfoText);
            ExpIconAsset = Assets.Get(Protos.Asset.CharacterLevelIcon);
            GoldIconAsset = Assets.Get(Protos.Asset.GoldCoin);
            PlatIconAsset = Assets.Get(Protos.Asset.PlatinumCoin);

            SkillIcons = Assets.Get(Protos.Asset.HudSkillIcons);
            MapNameIcon = Assets.Get(Protos.Asset.MapNameIcon);
            MapNameText = new UiText(UiFonts.HUDSkillText);

            MessageBoxContainer = new UiVerticalContainer();
            BacklogContainer = new UiVerticalContainer();

            MessageWindow = new HudMessageWindow(MessageBoxContainer, BacklogContainer);

            foreach (HudSkillIconType type in Enum.GetValues<HudSkillIconType>())
            {
                var uiText = new UiText(UiFonts.HUDSkillText);
                if (type > HudSkillIconType.Cha)
                    HudSkillContainer.AddLayout(LayoutType.Spacer, 5);
                HudSkillContainer.AddElement(uiText);
                SkillTexts[type] = uiText;
            }

            _field.OnScreenRefresh += OnScreenRefresh;
            UpdateTime();
        }

        private void OnScreenRefresh()
        {
            UpdateMinimap();
            UpdateBars();
            UpdateInfoTexts();
        }

        public void UpdateTime()
        {
            var date = _world.State.GameDate;
            DateText.Text = $"{date.Year}/{date.Month}/{date.Day}";
            ClockHand.SetHour(date.Hour);
        }

        public void UpdateMinimap()
        {
            if (_entMan.TryGetComponent<SpatialComponent>(GameSession.Player, out var spatial))
                Minimap.Refresh(_mapManager.ActiveMap?.TileMemory!, spatial.MapPosition);
        }

        private void UpdateBars()
        {
            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                HealthText.Text = $"{skills.HP}({skills.MaxHP})";
                ManaText.Text = $"{skills.MP}({skills.MaxMP})";
                HpBarEntry = new UiHelpers.DrawEntry(HpBarAsset, Math.Clamp((float)skills.HP / skills.MaxHP, 0f, 1f), new());
                MpBarEntry = new UiHelpers.DrawEntry(MpBarAsset, Math.Clamp((float)skills.MP / skills.MaxMP, 0f, 1f), new());
            }
        }

        private void UpdateInfoTexts()
        {
            if (_entMan.TryGetComponent<LevelComponent>(GameSession.Player, out var level))
            {
                ExpText.Text = $"{Loc.GetString("Elona.Hud.Info.Level")}{level.Level}/{level.ExperienceToNext}";
            }
            // Need components for Gold and Plat
            GoldText.Text = $"{GoldPlaceholder} {Loc.GetString("Elona.Hud.Info.Gold")}";
            PlatText.Text = $"{PlatPlaceholder} {Loc.GetString("Elona.Hud.Info.Platinum")}";

            if (_entMan.TryGetComponent<SkillsComponent>(GameSession.Player, out var skills))
            {
                foreach (HudSkillIconType type in Enum.GetValues<HudSkillIconType>())
                {
                    var text = SkillTexts[type];
                    PrototypeId<SkillPrototype>? protoId = type switch
                    {
                        HudSkillIconType.Str => Protos.Skill.AttrStrength,
                        HudSkillIconType.Con => Protos.Skill.AttrConstitution,
                        HudSkillIconType.Dex => Protos.Skill.AttrDexterity,
                        HudSkillIconType.Per => Protos.Skill.AttrPerception,
                        HudSkillIconType.Lea => Protos.Skill.AttrLearning,
                        HudSkillIconType.Wil => Protos.Skill.AttrWill,
                        HudSkillIconType.Mag => Protos.Skill.AttrMagic,
                        HudSkillIconType.Cha => Protos.Skill.AttrCharisma,
                        HudSkillIconType.Spd => Protos.Skill.AttrSpeed,
                        _ => null
                    };
                    if (protoId != null)
                    {
                        var skillLevel = skills.Skills[protoId.Value].Level;
                        text.Text = skillLevel.Buffed.ToString();
                        text.Color = GetColorForStat(skillLevel);
                    }
                    else
                    {
                        text.Color = Color.Black;
                        text.Text = $"{skills.DV.Buffed}/{skills.PV.Buffed}";
                    }
                    
                }
            }
            if (_mapManager.ActiveMap?.MapEntityUid != null)
                MapNameText.Text = _nameSystem.GetDisplayName(_mapManager.ActiveMap.MapEntityUid);
            else
                MapNameText.Text = string.Empty;
        }

        private Color GetColorForStat<T>(Stat<T> stat)
            where T : IComparable<T>
        {
            return stat.Buffed.CompareTo(stat.Base) switch
            {
                < 0 => Color.Red,
                > 0 => Color.Green,
                _ => Color.Black
            };
        }

        private string GetIconRegion(HudSkillIconType type)
        {
            return $"{(int)type}";
        }

        public void ToggleBacklog(bool visible)
        {
            ShowingBacklog = visible;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Minimap.SetSize(MinimapWidth - 4, MinimapHeight - 4);
            MessageWindow.SetSize(Width - MinimapWidth - 80, MessageWindow.Height);
            FpsCounter.SetSize(400, 500);
            MessageBoxBacking.SetSize(Width + MinimapWidth, 72);
            BacklogBacking.SetSize(width + MinimapWidth, 600);
            HudBar.SetSize(Width + MinimapWidth, UiHudBar.HudBarHeight);
            DateText.SetPreferredSize();
            HealthText.SetPreferredSize();
            ManaText.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Minimap.SetPosition(2, Height - MinimapHeight + 2);
            MessageWindow.SetPosition(X + 50, Y + Height - MessageWindow.Height - 10);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            MessageBoxBacking.SetPosition(MinimapWidth, Height - MinimapHeight);
            BacklogBacking.SetPosition(MinimapWidth + 10, Height - 470);
            BacklogContainer.SetPosition(MinimapWidth + 15, BacklogBacking.Y + 12);
            HudBar.SetPosition(MinimapWidth, Height - 18);
            MessageBoxContainer.SetPosition(BacklogContainer.X, Height - MinimapHeight + 4);
            DateText.SetPosition(120, 17);
            ClockHand.SetPosition(62, 48);
            HealthText.SetPosition(280, Height - MinimapHeight - 19);
            ManaText.SetPosition(420, Height - MinimapHeight - 19);
            ExpText.SetPosition(35, Height - MinimapHeight - 13);
            GoldText.SetPosition(590, Height - MinimapHeight - 13);
            PlatText.SetPosition(700, Height - MinimapHeight - 13);
            HudSkillContainer.SetPosition(305, Height - 14);
            HudSkillContainer.Relayout();
            MapNameText.SetPosition(160, Height - 14);
        }

        public override void Update(float dt)
        {
            MessageWindow.Update(dt);
            FpsCounter.Update(dt);
            MessageBoxContainer.Update(dt);
            BacklogContainer.Update(dt);
            DateText.Update(dt);
            ClockHand.Update(dt);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Color.White);
            MiniMapAsset.Draw(0, Height - MinimapHeight);
            Minimap.Draw();
            if (ShowingBacklog)
            { 
                BacklogBacking.Draw();
                BacklogContainer.Draw();
            }
            GraphicsEx.SetColor(Color.White);
            MessageBoxBacking.Draw();
            HudBar.Draw();
            MessageWindow.Draw();
            MessageBoxContainer.Draw();

            GraphicsEx.SetColor(Color.White);
            var barHeight = Height - MinimapHeight - 12;
            ExpIconAsset.Draw(5, barHeight - 4);
            ExpText.Draw();
            if (!ShowingBacklog)
            {
                BarAsset.Draw(260, barHeight);
                BarAsset.Draw(400, barHeight);
                HealthText.Draw();
                ManaText.Draw();
                Love.Graphics.SetColor(Color.White);
                var hpBarWidth = 83;
                if (HpBarEntry != null)
                    UiHelpers.DrawPercentageBar(HpBarEntry, new(277, barHeight + 5), HpBarEntry.HPRatio * hpBarWidth, Vector2i.Zero);
                if (MpBarEntry != null)
                    UiHelpers.DrawPercentageBar(MpBarEntry, new(417, barHeight + 5), MpBarEntry.HPRatio * hpBarWidth, new(hpBarWidth, 7));

                GoldIconAsset.Draw(560, barHeight - 4);
                PlatIconAsset.Draw(670, barHeight - 4);
                GoldText.Draw();
                PlatText.Draw();
            }

            FpsCounter.Draw();
            DateFrame.Draw(80, 8);
            ClockAsset.Draw(0, 0);
            ClockHand.Draw();
            DateText.Draw();
            HudSkillContainer.Draw();

            GraphicsEx.SetColor(Color.White);
            int iconIndex = 0, iconOffset = HudSkillContainer.XMinWidth, extraOffset = 0;
            foreach (HudSkillIconType type in Enum.GetValues<HudSkillIconType>())
            {
                if (type > HudSkillIconType.Cha)
                    extraOffset += 5;
                SkillIcons.DrawRegion(GetIconRegion(type), 285 + (iconIndex * iconOffset) + extraOffset, Height - 16);
                iconIndex++;
            }
            MapNameIcon.Draw(140, Height - 16);
            MapNameText.Draw();
        }

        public override void Dispose()
        {
            MessageWindow.Dispose();
            FpsCounter.Dispose();
            _field.OnScreenRefresh -= OnScreenRefresh;
        }
    }
}

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

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : UiLayer, IHudLayer
    {
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
        private IUiText DateText = default!;
        private ClockHand ClockHand = default!;
        
        private bool ShowingBacklog;

        private const int MinimapWidth = 122;
        private const int MinimapHeight = 88;

        public const int HudZOrder = 200000000;

        public HudLayer()
        {
            IoCManager.InjectDependencies(this);
            FpsCounter = new UiFpsCounter();
        }

        public void Initialize()
        {
            _world = EntitySystem.Get<WorldSystem>();
            DateText = new UiText();

            CanKeyboardFocus = true;
            MessageBoxBacking = new UiMessageWindowBacking();
            BacklogBacking = new UiMessageWindowBacking(UiMessageWindowBacking.MessageBackingType.Expanded);
            HudBar = new UiHudBar();
            MiniMapAsset = Assets.Get(Protos.Asset.HudMinimap);
            ClockAsset = Assets.Get(Protos.Asset.Clock);
            ClockHand = new ClockHand();
            DateFrame = Assets.Get(Protos.Asset.DateLabelFrame);

            MessageBoxContainer = new UiVerticalContainer();
            BacklogContainer = new UiVerticalContainer();

            MessageWindow = new HudMessageWindow(MessageBoxContainer, BacklogContainer);
            UpdateTime();
        }

        public void UpdateTime()
        {
            var date = _world.State.GameDate;
            DateText.Text = $"{date.Year}/{date.Month}/{date.Day}";
            ClockHand.SetHour(date.Hour);
        }

        public void ToggleBacklog(bool visible)
        {
            ShowingBacklog = visible;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(Width - MinimapWidth - 80, MessageWindow.Height);
            FpsCounter.SetSize(400, 500);
            MessageBoxBacking.SetSize(Width + MinimapWidth, 72);
            BacklogBacking.SetSize(width + MinimapWidth, 600);
            HudBar.SetSize(Width + MinimapWidth, UiHudBar.HudBarHeight);
            DateText.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            MessageWindow.SetPosition(X + 50, Y + Height - MessageWindow.Height - 10);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            MessageBoxBacking.SetPosition(MinimapWidth, Height - MinimapHeight);
            BacklogBacking.SetPosition(MinimapWidth + 10, Height - 470);
            BacklogContainer.SetPosition(MinimapWidth + 15, BacklogBacking.Y + 12);
            HudBar.SetPosition(MinimapWidth, Height - 18);
            MessageBoxContainer.SetPosition(BacklogContainer.X, Height - MinimapHeight + 4);
            DateText.SetPosition(120, 17);
            ClockHand.SetPosition(62, 48);
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
            MiniMapAsset.Draw(0, Height - MinimapHeight);
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
            FpsCounter.Draw();
            DateFrame.Draw(80, 8);
            ClockAsset.Draw(0, 0);
            ClockHand.Draw();
            DateText.Draw();
        }

        public override void Dispose()
        {
            MessageWindow.Dispose();
            FpsCounter.Dispose();
        }
    }
}

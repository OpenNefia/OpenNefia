using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : UiLayer, IHudLayer
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        public IHudMessageWindow MessageWindow { get; }
        private UiFpsCounter FpsCounter;
        private BaseDrawable MessageBoxBacking = default!;
        private BaseDrawable HudBar = default!;

        private IAssetInstance MiniMapAsset = default!;

        private const int MinimapWidth = 122;
        private const int MinimapHeight = 88;

        public const int HudZOrder = 10000000;

        public HudLayer()
        {
            IoCManager.InjectDependencies(this);
            MessageWindow = new SimpleMessageWindow();
            FpsCounter = new UiFpsCounter();
        }

        public void Initialize()
        {
            MessageBoxBacking = new UiMessageWindowBacking();
            HudBar = new UiHudBar();
            MiniMapAsset = Assets.Get(Protos.Asset.HudMinimap);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(_graphics.WindowSize.X - 100, 150);
            FpsCounter.SetSize(400, 500);
            MessageBoxBacking.SetSize(Width + MinimapWidth, 72);
            HudBar.SetSize(Width + MinimapWidth, UiHudBar.HudBarHeight);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            MessageWindow.SetPosition(X + 50, Y + Height - MessageWindow.Height - 10);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            MessageBoxBacking.SetPosition(MinimapWidth, Height - MinimapHeight);
            HudBar.SetPosition(MinimapWidth, Height - 18);
        }

        public override void Update(float dt)
        {
            MessageWindow.Update(dt);
            FpsCounter.Update(dt);
        }

        public override void Draw()
        {
            MiniMapAsset.Draw(0, Height - MinimapHeight);
            MessageBoxBacking.Draw();
            HudBar.Draw();
            MessageWindow.Draw();
            FpsCounter.Draw();
        }

        public override void Dispose()
        {
            MessageWindow.Dispose();
            FpsCounter.Dispose();
        }
    }
}

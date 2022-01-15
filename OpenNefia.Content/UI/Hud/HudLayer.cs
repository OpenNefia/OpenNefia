using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : UiLayerWithResult<UINone, UINone>, IHudLayer
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        public IHudMessageWindow MessageWindow { get; }
        private UiFpsCounter FpsCounter;
        private UiMessageWindowBacking Test = default!;

        public HudLayer()
        {
            IoCManager.InjectDependencies(this);

            MessageWindow = new SimpleMessageWindow();
            FpsCounter = new UiFpsCounter();
        }

        public override void Initialize(UINone args)
        {
            base.Initialize(args);
            Test = new UiMessageWindowBacking();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(_graphics.WindowSize.X - 100, 150);
            FpsCounter.SetSize(400, 500);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            MessageWindow.SetPosition(X + 50, Y + Height - MessageWindow.Height - 10);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
        }

        public override void Update(float dt)
        {
            MessageWindow.Update(dt);
            FpsCounter.Update(dt);
        }

        public override void Draw()
        {
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

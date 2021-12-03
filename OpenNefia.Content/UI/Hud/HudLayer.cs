using OpenNefia.Content.UI.Element;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : BaseUiLayer<UiNoResult>, IHudLayer
    {
        public IHudMessageWindow MessageWindow { get; }
        private UiFpsCounter FpsCounter;

        public HudLayer()
        {
            MessageWindow = new SimpleMessageWindow();
            FpsCounter = new UiFpsCounter();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(Love.Graphics.GetWidth() - 100, 150);
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

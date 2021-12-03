using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Hud
{
    public class HudLayer : BaseUiLayer<UiNoResult>, IHudLayer
    {
        public IHudMessageWindow MessageWindow { get; }

        public HudLayer()
        {
            MessageWindow = new SimpleMessageWindow();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(Love.Graphics.GetWidth() - 100, 150);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            MessageWindow.SetPosition(X + 50, Y + Height - MessageWindow.Height - 10);
        }

        public override void Update(float dt)
        {
            MessageWindow.Update(dt);
        }

        public override void Draw()
        {
            MessageWindow.Draw();
        }

        public override void Dispose()
        {
            MessageWindow.Dispose();
        }
    }
}

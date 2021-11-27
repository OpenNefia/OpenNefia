using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Hud
{
    public class HudLayer : BaseUiLayer<UiNoResult>
    {
        internal IHudMessageWindow MessageWindow { get; }

        public HudLayer()
        {
            this.MessageWindow = new SimpleMessageWindow();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            this.MessageWindow.SetSize(Love.Graphics.GetWidth() - 100, 150);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.MessageWindow.SetPosition(this.X + 50, this.Y + this.Height - this.MessageWindow.Height - 10);
        }

        public override void Update(float dt)
        {
            this.MessageWindow.Update(dt);
        }

        public override void Draw()
        {
            this.MessageWindow.Draw();
        }

        public override void Dispose()
        {
            this.MessageWindow.Dispose();
        }
    }
}

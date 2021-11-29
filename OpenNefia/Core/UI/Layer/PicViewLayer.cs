using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    internal partial class PicViewLayer : BaseUiLayer<UiNoResult>
    {
        public Image Image { get; private set; }
        public bool DrawBorder { get; set; }

        private UiScroller Scroller;

        public PicViewLayer(Love.Image image)
        {
            Image = image;
            DrawBorder = true;

            Scroller = new UiScroller();
            
            Scroller.BindKeys(this);
            this.Keybinds[Keybind.Escape] += (_) => this.Cancel();
            this.Keybinds[Keybind.Cancel] += (_) => this.Cancel();
        }

        public override void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            var rect = UiUtils.GetCenteredParams(this.Image.GetWidth(), this.Image.GetHeight());
            x = rect.X;
            y = rect.Y;
            width = rect.Width;
            height = rect.Height;
        }

        public override void Update(float dt)
        {
            Scroller.GetPositionDiff(dt, out var dx, out var dy);
            this.SetPosition(this.Left + dx, this.Top + dy);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.Black);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.Left, this.Top, this.Width, this.Height);

            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.Draw(this.Image, this.Left, this.Top);

            if (this.DrawBorder)
            {
                Love.Graphics.Rectangle(Love.DrawMode.Line, this.Left, this.Top, this.Width, this.Height);
            }
        }
    }
}

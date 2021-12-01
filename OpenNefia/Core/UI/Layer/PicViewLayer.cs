using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
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
            this.Keybinds[CoreKeybinds.Escape] += (_) => this.Cancel();
            this.Keybinds[CoreKeybinds.Cancel] += (_) => this.Cancel();
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(this.Image.GetWidth(), this.Image.GetHeight(), out bounds);
        }

        public override void Update(float dt)
        {
            Scroller.GetPositionDiff(dt, out var dx, out var dy);
            this.SetPosition(this.X + dx, this.Y + dy);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.Black);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.X, this.Y, this.Width, this.Height);

            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.Draw(this.Image, this.X, this.Y);

            if (this.DrawBorder)
            {
                Love.Graphics.Rectangle(Love.DrawMode.Line, this.X, this.Y, this.Width, this.Height);
            }
        }
    }
}

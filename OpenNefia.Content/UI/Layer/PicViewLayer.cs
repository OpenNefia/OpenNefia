using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer
{
    internal partial class PicViewLayer : BaseUiLayer<UiNoResult>
    {
        public Image Image { get; private set; }
        public bool DrawBorder { get; set; }

        private UiScroller Scroller;

        public PicViewLayer(Image image)
        {
            Image = image;
            DrawBorder = true;

            Scroller = new UiScroller();

            Scroller.BindKeys(this);
            Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(Image.GetWidth(), Image.GetHeight(), out bounds);
        }

        public override void Update(float dt)
        {
            Scroller.GetPositionDiff(dt, out var dx, out var dy);
            SetPosition(X + dx, Y + dy);
        }

        public override void Draw()
        {
            Graphics.SetColor(Love.Color.Black);
            Graphics.Rectangle(DrawMode.Fill, X, Y, Width, Height);

            Graphics.SetColor(Love.Color.White);
            Graphics.Draw(Image, X, Y);

            if (DrawBorder)
            {
                Graphics.Rectangle(DrawMode.Line, X, Y, Width, Height);
            }
        }
    }
}

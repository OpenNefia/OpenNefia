using Love;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer
{
    internal partial class PicViewLayer : UiLayerWithResult<Image, UINone>
    {
        public Image Image { get; private set; } = default!;
        public bool DrawBorder { get; set; }

        private UiScroller Scroller;

        public PicViewLayer()
        {
            DrawBorder = true;
            Scroller = new UiScroller();

            OnKeyBindDown += HandleKeyBindDown;
            OnKeyBindDown += Scroller.HandleKeyBindDown;
            OnKeyBindUp += Scroller.HandleKeyBindUp;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }
        
        public override void Initialize(Image image)
        {
            Image = image;
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
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
            GraphicsS.RectangleS(UIScale, DrawMode.Fill, X, Y, Width, Height);

            Graphics.SetColor(Love.Color.White);
            Graphics.Draw(Image, PixelX, PixelY);

            if (DrawBorder)
            {
                GraphicsS.RectangleS(UIScale, DrawMode.Line, X, Y, Width, Height);
            }
        }
    }
}

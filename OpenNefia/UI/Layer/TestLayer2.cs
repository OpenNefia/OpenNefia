using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections;

namespace OpenNefia.Core.UI.Layer
{
    public class TestLayer2 : BaseUiLayer<string>
    {
        public TestLayer2()
        {
        }

        private int SquareX = 0;

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(400, 300, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
        }

        public override void Update(float dt)
        {
            if (this.SquareX > 200)
            {
                this.SquareX = 0;
            }
            else
            {
                this.SquareX++;
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(1f, 1f, 1f);
            Love.Graphics.Rectangle(DrawMode.Fill, 100, 100, 100, 100);
            Love.Graphics.SetColor(1f, 0, 1f);
            Love.Graphics.Rectangle(DrawMode.Fill, 50 + this.SquareX, 50, 100, 100);
        }
    }
}

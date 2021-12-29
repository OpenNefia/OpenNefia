using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Layer
{
    public class TestLayer2 : UiLayerWithResult<string>
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
            if (SquareX > 200)
            {
                SquareX = 0;
            }
            else
            {
                SquareX++;
            }
        }

        public override void Draw()
        {
            Graphics.SetColor(1f, 1f, 1f);
            Graphics.Rectangle(DrawMode.Fill, 100, 100, 100, 100);
            Graphics.SetColor(1f, 0, 1f);
            Graphics.Rectangle(DrawMode.Fill, 50 + SquareX, 50, 100, 100);
        }
    }
}

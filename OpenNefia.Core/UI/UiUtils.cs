using Love;
using OpenNefia.Core.Game;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
{
    public static class UiUtils
    {
        public static string GetKeyName(Input.Keyboard.Key key)
        {
            return Enum.GetName(typeof(Input.Keyboard.Key), key)!.ToLowerInvariant();
        }

        public static bool IsPointInVisibleScreen(this IGraphics gr, Vector2i screenPos)
        {
            return screenPos.X >= 0 && screenPos.Y >= 0 && screenPos.X < gr.WindowSize.X && screenPos.Y < gr.WindowSize.Y - Constants.INF_MSGH;
        }

        public static void GetCenteredParams(int width, int height, out UIBox2i bounds)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            var (windowW, windowH) = graphics.WindowSize;

            var ingame = false;
            var x = (windowW - width) / 2;
            var y = 0;
            if (ingame)
            {
                var coords = GameSession.Coords;
                var tiledHeight = windowH / coords.TileSize.Y;
                y = ((tiledHeight - 2) * coords.TileSize.Y - height) / 2 + 8;
            }
            else
            {
                y = (windowH - height) / 2;
            }

            bounds = UIBox2i.FromDimensions(x, y, width, height);
        }

        public static void DebugDraw(IDrawable elem)
        {
            Love.Graphics.SetColor(Love.Color.Red);
            Love.Graphics.Rectangle(Love.DrawMode.Line, elem.X, elem.Y, elem.Width, elem.Height);
            Love.Graphics.SetColor(Love.Color.Blue);
            Love.Graphics.Line(elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height);
        }

        public static string DisplayWeight(int weight)
        {
            var integer = Math.Abs(weight / 1000);
            var fractional = Math.Abs((weight % 1000) / 100);
            return $"{integer}.{fractional}s";
        }

        public static Vector2i NotePosition(UIBox2i bounds, IDrawable text, int xOffset = 0)
        {
            return new(bounds.Right - text.Width - 140 - xOffset,
                       bounds.Bottom - 65 - bounds.Height % 8);
        }
    }
}

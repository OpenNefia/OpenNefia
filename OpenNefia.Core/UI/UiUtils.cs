using Love;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
{
    public static class UiUtils
    {
        public static string GetKeyName(Keys keyAndModifiers)
        {
            return Enum.GetName(typeof(Keys), keyAndModifiers)!.ToLowerInvariant();
        }

        public static bool IsPointInVisibleScreen(Vector2i screenPos)
        {
            return screenPos.X >= 0 && screenPos.Y >= 0 && screenPos.X < Love.Graphics.GetWidth() && screenPos.Y < Love.Graphics.GetHeight() - Constants.INF_MSGH;
        }

        public static void GetCenteredParams(int width, int height, out UIBox2i bounds)
        {
            var ingame = false;
            var x = (Love.Graphics.GetWidth() - width) / 2;
            var y = 0;
            if (ingame)
            {
                var coords = GameSession.Coords;
                var tiledHeight = Love.Graphics.GetHeight() / coords.TileSize.Y;
                y = ((tiledHeight - 2) * coords.TileSize.Y - height) / 2 + 8;
            }
            else
            {
                y = (Love.Graphics.GetHeight() - height) / 2;
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
    }
}

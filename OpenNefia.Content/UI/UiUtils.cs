using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI
{
    public static class UiUtils
    {
        public static string GetKeyName(Core.Input.Keyboard.Key key)
        {
            return Enum.GetName(typeof(Core.Input.Keyboard.Key), key)!.ToLowerInvariant();
        }

        public static bool IsPointInVisibleScreen(this IGraphics gr, Vector2i screenPos)
        {
            return screenPos.X >= 0 && screenPos.Y >= 0 && screenPos.X < gr.WindowPixelSize.X && screenPos.Y < gr.WindowPixelSize.Y - Constants.INF_MSGH;
        }

        public static void GetCenteredParams(Vector2 size, out UIBox2 bounds, float yOffset = 0f)
            => GetCenteredParams(size.X, size.Y, out bounds, yOffset);

        public static void GetCenteredParams(float width, float height, out UIBox2 bounds, float yOffset = 0f)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            var coords = IoCManager.Resolve<ICoords>();
            var field = IoCManager.Resolve<IFieldLayer>();

            var (windowW, windowH) = graphics.WindowSize;

            var x = (windowW - width) / 2;
            float y;

            if (field.IsInGame())
            {
                var tiledHeight = windowH / coords.TileSize.Y;
                y = ((tiledHeight - 2) * coords.TileSize.Y - height) / 2 + 8;
            }
            else
            {
                y = (windowH - height) / 2;
            }

            y += yOffset;

            bounds = UIBox2.FromDimensions(x, y, width, height);
        }

        public static void DebugDraw(IDrawable elem)
        {
            Love.Graphics.SetColor(Love.Color.Red);
            GraphicsS.RectangleS(elem.UIScale, Love.DrawMode.Line, elem.X, elem.Y, elem.Width, elem.Height);
            Love.Graphics.SetColor(Love.Color.Blue);
            Love.Graphics.Line(elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height);
        }

        public static string DisplayWeight(int weight)
        {
            var integer = Math.Abs(weight / 1000);
            var fractional = Math.Abs(weight % 1000 / 100);
            return $"{integer}.{fractional}s";
        }

        public static Vector2 NotePosition(UIBox2 bounds, IDrawable text, float xOffset = 0f)
        {
            return new(bounds.Right - text.Width - 140 - xOffset,
                       bounds.Bottom - 65 - bounds.Height % 8);
        }
    }
}

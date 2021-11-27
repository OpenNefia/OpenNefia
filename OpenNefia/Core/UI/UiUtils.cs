using Love;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Element.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public static class UiUtils
    {
        public static string GetKeyName(Keys keyAndModifiers)
        {
            return Enum.GetName(typeof(Keys), keyAndModifiers)!.ToLowerInvariant();
        }

        public static bool IsPointInVisibleScreen(int screenX, int screenY)
        {
            return screenX >= 0 && screenY >= 0 && screenX < Love.Graphics.GetWidth() && screenY < Love.Graphics.GetHeight() - Constants.INF_MSGH;
        }

        public static Rectangle GetCenteredParams(int width, int height)
        {
            var ingame = false;
            var x = (Love.Graphics.GetWidth() - width) / 2;
            var y = 0;
            if (ingame)
            {
                var coords = GraphicsEx.Coords;
                var tiledHeight = Love.Graphics.GetHeight() / coords.TileHeight;
                y = ((tiledHeight - 2) * coords.TileHeight - height) / 2 + 8;
            }
            else
            {
                y = (Love.Graphics.GetHeight() - height) / 2;
            }

            return new Rectangle(x, y, width, height);
        }

        public static void DebugDraw(IDrawable elem)
        {
            Graphics.SetColor(Love.Color.Red);
            GraphicsEx.LineRect(elem.X, elem.Y, elem.Width, elem.Height);
            Graphics.SetColor(Love.Color.Blue);
            Graphics.Line(elem.X, elem.Y, elem.X + elem.Width, elem.Y + elem.Height);
            //Graphics.Print($"{elem}", elem.X, elem.Y);
        }
    }
}

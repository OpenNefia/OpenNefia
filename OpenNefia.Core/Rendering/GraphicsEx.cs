using Love;
using OpenNefia.Core.Game;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public static class GraphicsEx
    {
        public static void DrawImage(Love.Image image, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            var sx = 1f;
            var sy = 1f;

            if (width > 0)
            {
                sx = width / image.GetWidth();
            }
            if (height > 0)
            {
                sy = height / image.GetHeight();
            }

            var ox = 0f;
            var oy = 0f;

            if (centered)
            {
                ox = image.GetWidth() / 2f;
                oy = image.GetHeight() / 2f;
            }

            Love.Graphics.Draw(image, x, y, rotation, sx, sy, ox, oy);
        }

        public static Vector2i GetViewportSize() => new Vector2i(Love.Graphics.GetWidth(), Love.Graphics.GetHeight());

        public static void GetWindowTiledSize(this ICoords coords, out Vector2i windowTiledSize)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            coords.GetTiledSize(graphics.WindowSize, out windowTiledSize);
        }

        public static void DrawSpriteBatch(Love.SpriteBatch batch, float x, float y, float width, float height, float rotation = 0)
        {
            Rectangle? scissor = null;

            if (width > 0 || height > 0)
            {
                // Sprite batches will ignore the width and height of
                // love.graphics.draw; we have to manually set the scissor.
                scissor = Love.Graphics.GetScissor();
                Love.Graphics.SetScissor((int)x, (int)y, (int)width, (int)height);
            }

            Love.Graphics.Draw(batch, x, y, rotation);

            if (scissor.HasValue)
                Love.Graphics.SetScissor(scissor.Value);
        }

        public static void DrawImageRegion(Love.Image image, Love.Quad quad, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            var viewport = quad.GetViewport();

            var sx = 1f;
            var sy = 1f;

            if (width > 0)
            {
                sx = width / viewport.Width;
            }
            if (height > 0)
            {
                sy = height / viewport.Height;
            }

            var ox = 0f;
            var oy = 0f;

            if (centered)
            {
                ox = viewport.Width / 2f;
                oy = viewport.Height / 2f;
            }

            Love.Graphics.Draw(quad, image, x, y, rotation, sx, sy, ox, oy);
        }

        internal static void SetColor(object colorTextBlack)
        {
            throw new NotImplementedException();
        }

        internal static void SetDefaultFilter(ImageFilter filter)
        {
            Love.Graphics.SetDefaultFilter(filter.Min, filter.Mag, filter.Anisotropy);
        }

        /// <summary>
        /// Like <see cref="Love.Graphics.SetColor"/>, but uses byte values.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="a"></param>
        public static void SetColor(int r, int g, int b, int a = 255)
        {
            Love.Graphics.SetColor((float)r / 255f, (float)g / 255f, (float)b / 255f, (float)a / 255f);
        }

        public static void SetColor(Love.Color color) => Love.Graphics.SetColor(color);
        public static void SetColor(Maths.Color color) => Love.Graphics.SetColor(color.R, color.G, color.B, color.A);

        public static void SetFont(FontSpec spec)
        {
            Love.Graphics.SetColor(spec.Color);
            Love.Graphics.SetFont(spec.LoveFont);
        }
    }
}

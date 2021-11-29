using Love;
using OpenNefia.Core.Data;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
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

        internal static void GetWindowTiledSize(out Vector2i windowTiledSize)
        {
            GameSession.Coords.GetTiledSize(GetViewportSize(), out windowTiledSize);
        }

        public static void DrawSpriteBatch(Love.SpriteBatch batch, float x = 0, float y = 0, float width = 0, float height = 0, float rotation = 0)
        {
            // Sprite batches will ignore the width and height of
            // love.graphics.draw; we have to manually set the scissor.
            var scissor = Love.Graphics.GetScissor();
            SetScissor((int)x, (int)y, (int)width, (int)height);

            Love.Graphics.Draw(batch, x, y, rotation);

            SetScissor(scissor);
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
            Love.Graphics.SetFont(spec.LoveFont);
        }

        /// <summary>
        /// BUG: <see cref="Love.Graphics.SetScissor"/> doesn't distinguish between a zero-sized Rectangle and no scissor.
        /// This function is a temporary workaround.
        /// </summary>
        /// <param name="rectangle"></param>
        public static void SetScissor(Love.Rectangle rectangle)
        {
            SetScissor(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        /// <summary>
        /// BUG: <see cref="Love.Graphics.SetScissor"/> doesn't distinguish between a zero-sized scissor and no scissor.
        /// This function is a temporary workaround.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SetScissor(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            if (width <= 0 && height <= 0)
            {
                Love.Graphics.SetScissor();
            }
            else
            {
                Love.Graphics.SetScissor(x, y, width, height);
            }
        }
    }
}

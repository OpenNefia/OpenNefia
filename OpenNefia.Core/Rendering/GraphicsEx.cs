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
        public static void DrawImageS(float uiScale, Love.Texture image, float x, float y, float? width = null, float? height = null, bool centered = false, float rotation = 0, Maths.Vector2 originOffset = default)
        {
            if (width == null)
                width = image.GetWidth();
            if (height == null)
                height = image.GetHeight();

            DrawImage(image, x * uiScale, y * uiScale, width.Value * uiScale, height.Value * uiScale, centered, rotation, originOffset * uiScale);
        }

        public static void Rectangle(DrawMode fill, UIBox2 box)
        {
            Love.Graphics.Rectangle(fill, box.Left, box.Top, box.Width, box.Height);
        }

        public static void DrawImage(Love.Texture image, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0, Maths.Vector2 originOffset = default)
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

            if (centered)
            {
                originOffset.X += image.GetWidth() / 2f;
                originOffset.Y += image.GetHeight() / 2f;
            }

            Love.Graphics.Draw(image, x, y, rotation, sx, sy, originOffset.X, originOffset.Y);
        }

        public static void DrawImage(Love.Quad quad, Love.Texture image, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0, Maths.Vector2 originOffset = default)
        {
            var sx = 1f;
            var sy = 1f;

            var quadRect = quad.GetViewport();

            if (width > 0)
            {
                sx = width / quadRect.Width;
            }
            if (height > 0)
            {
                sy = height / quadRect.Height;
            }

            if (centered)
            {
                originOffset.X += quadRect.Width / 2f;
                originOffset.Y += quadRect.Height / 2f;
            }

            Love.Graphics.Draw(quad, image, x, y, rotation, sx, sy, originOffset.X, originOffset.Y);
        }   

        public static void DrawSpriteBatchS(float uiScale, Love.SpriteBatch batch, float x, float y, float? width = null, float? height = null, float rotation = 0)
        {
            var oldScissor = Love.Graphics.GetScissor();
            if (width != null && height != null)
            {
                Love.Graphics.SetScissor(new Love.RectangleF(x * uiScale, y * uiScale, width.Value * uiScale, height.Value * uiScale));
            }
            Love.Graphics.Draw(batch, x * uiScale, y * uiScale, rotation);

            if (oldScissor != null)
                Love.Graphics.SetScissor(oldScissor.Value);
            else
                Love.Graphics.SetScissor();
        }

        public static void DrawImageRegion(Love.Image image, Love.Quad quad, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0, Maths.Vector2 originOffset = default)
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

            if (centered)
            {
                originOffset.X += viewport.Width / 2f;
                originOffset.Y += viewport.Height / 2f;
            }

            Love.Graphics.Draw(quad, image, x, y, rotation, sx, sy, originOffset.X, originOffset.Y);
        }

        internal static void SetColor(object colorTextBlack)
        {
            throw new NotImplementedException();
        }

        public static ImageFilter GetDefaultFilter()
        {
            Love.Graphics.GetDefaultFilter(out var min, out var mag, out var anisotropy);
            return new ImageFilter(min, mag, anisotropy);
        }

        public static void SetDefaultFilter(ImageFilter filter)
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

        public static void WithCanvas(Love.Canvas canvas, Action callback)
        {
            var oldCanvas = Love.Graphics.GetCanvas();
            var oldColor = Love.Graphics.GetColor();
            Love.Graphics.GetBlendMode(out var oldBlendMode, out var oldBlendAlphaMode);

            Love.Graphics.SetCanvas(canvas);
            Love.Graphics.SetBlendMode(BlendMode.Alpha);

            callback();

            Love.Graphics.SetCanvas(oldCanvas);
            Love.Graphics.SetColor(oldColor);
            Love.Graphics.SetBlendMode(oldBlendMode, oldBlendAlphaMode);
        }
    }
}

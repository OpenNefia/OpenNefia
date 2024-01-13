using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using System.Runtime.CompilerServices;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Helper functions that wrap things in <see cref="Love.Graphics"/>
    /// to support UI scaling.
    /// </summary>
    public static class GraphicsS
    {
        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Draw(Love.Drawable, float, float, float, float, float, float, float, float, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DrawS(float uiScale, Love.Drawable drawable, float vx, float vy, float angle = 0, float vsx = 1, float vsy = 1, float vox = 0, float voy = 0, float vkx = 0, float vky = 0)
        {
            Love.Graphics.Draw(drawable, vx * uiScale, vy * uiScale, angle, vsx * uiScale, vsy * uiScale, vox * uiScale, voy * uiScale, vkx * uiScale, vky * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Rectangle(Love.DrawMode, float, float, float, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RectangleS(float uiScale, Love.DrawMode drawMode, float vx, float vy, float vw, float vh)
        {
            Love.Graphics.Rectangle(drawMode, vx * uiScale, vy * uiScale, vw * uiScale, vh * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Rectangle(Love.DrawMode, float, float, float, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RectangleS(float uiScale, Love.DrawMode drawMode, Vector2 pos, Vector2 size)
        {
            Love.Graphics.Rectangle(drawMode, pos.X * uiScale, pos.Y * uiScale, size.X * uiScale, size.Y * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Rectangle(Love.DrawMode, float, float, float, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RectangleS(float uiScale, Love.DrawMode drawMode, UIBox2 box)
        {
            Love.Graphics.Rectangle(drawMode, box.Left * uiScale, box.Top * uiScale, box.Width * uiScale, box.Height * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Line(float[])"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LineS(float uiScale, float vx1, float vy1, float vx2, float vy2)
        {
            Love.Graphics.Line(vx1 * uiScale, vy1 * uiScale, vx2 * uiScale, vy2 * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Translate(float, float)"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void TranslateS(float uiScale, float vx, float vy)
        {
            Love.Graphics.Translate(vx * uiScale, vy * uiScale);
        }

        /// <summary>
        /// Scale-aware version of <see cref="Love.Graphics.Print(string, float, float, float, float, float, float, float, float, float)"/>. 
        /// </summary>
        public static void PrintS(float uiScale, string text, float vx, float vy)
        {
            Love.Graphics.Print(text, vx * uiScale, vy * uiScale);
        }

        #region Extension Methods

        /// <summary>
        /// Gets the width of the text rendered with this font in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetWidthV(this Love.Font font, float uiScale, string text)
        {
            return font.GetWidth(text) / uiScale;
        }

        /// <summary>
        /// Gets the height of the font in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetHeightV(this Love.Font font, float uiScale)
        {
            return (int)Math.Round(font.GetHeight() * IoCManager.Resolve<IConfigurationManager>().GetCVar(CVars.DisplayFontHeightScale)) / uiScale;
        }

        /// <summary>
        /// Wraps a string in virtual pixel increments.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Tuple<int, string[]> GetWrapS(this Love.Font font, float uiScale, string text, float maxWidth)
        {
            return font.GetWrap(text, maxWidth * uiScale);
        }

        /// <summary>
        /// Gets the width of this text in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetWidthV(this Love.Text text, float uiScale)
        {
            return text.GetWidth() / uiScale;
        }

        /// <summary>
        /// Gets the height of this text in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetHeightV(this Love.Text text, float uiScale)
        {
            return text.GetHeight() / uiScale;
        }

        /// <summary>
        /// Gets the ascent of the font in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetAscentV(this Love.Font font, float uiScale)
        {
            return font.GetAscent() / uiScale;
        }

        /// <summary>
        /// Gets the descent of the font in virtual pixels.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetDescentV(this Love.Font font, float uiScale)
        {
            return font.GetDescent() / uiScale;
        }

        #endregion

        #region Other

        /// <summary>
        /// TODO stop using static methods and place this in a DrawingHandle class.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void SetColorTinted(WispControl control, Color color)
        {
            Love.Graphics.SetColor(color * control.WispRootLayer!.GlobalTint);
        }

        #endregion
    }
}

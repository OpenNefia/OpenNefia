using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Helper functions that wrap things in <see cref="Love.Graphics"/>
    /// to support UI scaling.
    /// </summary>
    public static class GraphicsS
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void DrawS(float uiScale, Love.Drawable drawable, float vx, float vy, float angle = 0, float vsx = 1, float vsy = 1, float vox = 0, float voy = 0, float vkx = 0, float vky = 0)
        {
            Love.Graphics.Draw(drawable, vx * uiScale, vy * uiScale, angle, vsx * uiScale, vsy * uiScale, vox * uiScale, voy * uiScale, vkx * uiScale, vky * uiScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void RectangleS(float uiScale, Love.DrawMode drawMode, float vx, float vy, float vw, float vh)
        {
            Love.Graphics.Rectangle(drawMode, vx * uiScale, vy * uiScale, vw * uiScale, vh * uiScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LineS(float uiScale, float vx1, float vy1, float vx2, float vy2)
        {
            Love.Graphics.Line(vx1 * uiScale, vy1 * uiScale, vx2 * uiScale, vy2 * uiScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void TranslateS(float uiScale, float vx, float vy)
        {
            Love.Graphics.Translate(vx * uiScale, vy * uiScale);
        }

        #region Extension Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetWidthV(this Love.Font font, float uiScale, string text)
        {
            return font.GetWidth(text) / uiScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetHeightV(this Love.Font font, float uiScale)
        {
            return font.GetHeight() / uiScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static Tuple<int, string[]> GetWrapS(this Love.Font font, float uiScale, string text, float maxWidth)
        {
            return font.GetWrap(text, maxWidth * uiScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetWidthV(this Love.Text text, float uiScale)
        {
            return text.GetWidth() / uiScale;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static float GetHeightV(this Love.Text text, float uiScale)
        {
            return text.GetHeight() / uiScale;
        }

        #endregion
    }
}

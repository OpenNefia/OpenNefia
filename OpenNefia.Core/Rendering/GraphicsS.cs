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
        public static void RectangleS(float uiScale, Love.DrawMode drawMode, float vx, float vy, float vw, float vh)
        {
            Love.Graphics.Rectangle(drawMode, vx * uiScale, vy * uiScale, vw * uiScale, vh * uiScale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public static void LineS(float uiScale, float vx1, float vy1, float vx2, float vy2)
        {
            Love.Graphics.Line(vx1 * uiScale, vy1 * uiScale, vx2 * uiScale, vy2 * uiScale);
        }
    }
}

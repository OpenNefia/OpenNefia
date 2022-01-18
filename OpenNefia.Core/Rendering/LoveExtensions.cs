using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public static class LoveExtensions
    {
        public static void Set(this Love.Text text, string str, Love.Color? color = null)
        {
            text.Set(new Love.ColoredStringArray(Love.ColoredString.Create(str, color ?? Love.Color.White)));
        }

        public static float GetVirtualWidth(this Love.Font font, float uiScale, string text)
        {
            return font.GetWidth(text) / uiScale;
        }

        public static float GetVirtualHeight(this Love.Font font, float uiScale)
        {
            return font.GetHeight() / uiScale;
        }
    }
}

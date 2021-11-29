using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    public class FontManager : IFontManager
    {
        [Dependency] private readonly ILocalizationManager _localization = default!;

        private static Dictionary<int, Love.Font> _fontCache = new();
        private static ResourcePath _fallbackFontPath = new ResourcePath("Assets/Core/Font/kochi-gothic-subst.ttf");

        public Love.Font GetFont(FontSpec spec)
        {
            var size = _localization.IsFullwidth() ? spec.Size : spec.SmallSize;

            if (_fontCache.TryGetValue(size, out Love.Font? cachedFont))
            {
                return cachedFont;
            }

            var fontFilepath = _fallbackFontPath;

            var font = Love.Graphics.NewFont(fontFilepath.ToString(), size);
            _fontCache[size] = font;

            return font;
        }

        public void Clear()
        {
            _fontCache.Clear();
        }
    }
}

using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    public class FontManager : IFontManager
    {
        [Dependency] private readonly ILocalizationManager _localization = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private ResourcePath _fallbackFontPath = new("/Font/Core/kochi-gothic-subst.ttf");
        private static Dictionary<int, Love.Font> _fontCache = new();

        public void Initialize()
        {
            // Use MS Gothic if it's available.
            // This is so I can test compatibility with vanilla.
            var msGothic = new ResourcePath("/Font/Core/MS-Gothic.ttf");
            if (_resourceCache.ContentFileExists(msGothic))
                _fallbackFontPath = msGothic;
        }

        public Love.Font GetFont(FontSpec spec)
        {
            var size = _localization.IsFullwidth() ? spec.Size : spec.SmallSize;

            if (_fontCache.TryGetValue(size, out Love.Font? cachedFont))
            {
                return cachedFont;
            }

            var fontFilepath = _fallbackFontPath;

            var fileData = _resourceCache.GetResource<LoveFileDataResource>(fontFilepath);
            var rasterizer = Love.Font.NewTrueTypeRasterizer(fileData, size);
            var font = Love.Graphics.NewFont(rasterizer);

            font.SetFilter(Love.FilterMode.Nearest, Love.FilterMode.Nearest, 1);
            _fontCache[size] = font;

            return font;
        }

        public void Clear()
        {
            _fontCache.Clear();
        }
    }
}

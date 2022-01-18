using OpenNefia.Core.Configuration;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    public class FontManager : IFontManager
    {
        [Dependency] private readonly ILocalizationManager _localization = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private sealed record FontCacheEntry(FontSpec FontSpec, Love.Font LoveFont);

        private ResourcePath _fallbackFontPath = new("/Font/Core/kochi-gothic-subst.ttf");
        private static Dictionary<int, FontCacheEntry> _fontCache = new();
        private static HashSet<FontSpec> _fontSpecs = new();

        public void Initialize()
        {
            // Use MS Gothic if it's available.
            // This is so I can test compatibility with vanilla.
            var msGothic = new ResourcePath("/Font/Core/MS-Gothic.ttf");
            if (_resourceCache.ContentFileExists(msGothic))
                _fallbackFontPath = msGothic;

            _localization.OnLanguageSwitched += HandleLanguageSwitched;

            _config.OnValueChanged(CVars.DisplayUIScale, OnConfigDisplayUIScaleChanged);
        }

        /// <summary>
        /// Clear the cache so that fonts can be rebuilt with the proper size
        /// for the current language.
        /// </summary>
        private void HandleLanguageSwitched(PrototypeId<LanguagePrototype> _)
        {
            ClearCache();
        }

        private void OnConfigDisplayUIScaleChanged(float _)
        {
            ClearCache();
        }

        private void ClearCache()
        {
            foreach (var spec in _fontSpecs)
            {
                spec.ClearCachedFont();
            }

            _fontCache.Clear();
        }

        public Love.Font GetFont(FontSpec spec) => GetFont(spec, _config.GetCVar(CVars.DisplayUIScale));

        // TODO: Needs UI scale passed to it
        public Love.Font GetFont(FontSpec spec, float uiScale)
        {
            _fontSpecs.Add(spec);

            var size = _localization.IsFullwidth() ? spec.Size : spec.SmallSize;
            size = (int)(size * uiScale);

            if (_fontCache.TryGetValue(size, out var cachedEntry))
            {
                return cachedEntry.LoveFont;
            }

            var fontFilepath = _fallbackFontPath;

            var fileData = _resourceCache.GetResource<LoveFileDataResource>(fontFilepath);
            var rasterizer = Love.Font.NewTrueTypeRasterizer(fileData, size);
            var font = Love.Graphics.NewFont(rasterizer);
            var fontCacheEntry = new FontCacheEntry(spec, font);

            font.SetFilter(Love.FilterMode.Nearest, Love.FilterMode.Nearest, 1);
            _fontCache[size] = fontCacheEntry;

            return font;
        }

        public void Clear()
        {
            _fontCache.Clear();
            _fontSpecs.Clear();
        }
    }
}

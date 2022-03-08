using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using NLua;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Wisp.Drawing;

namespace OpenNefia.Core.UI.Wisp.Styling
{
    public interface IStylesheetManager
    {
        void Initialize();

        /// <summary>
        /// Gets a style fallback.
        /// </summary>
        /// <remarks>
        /// Only to be used as a last resort; if this gets called then 
        /// fallback stylesheets were not configured correctly.
        /// </remarks>
        T GetStyleFallback<T>();
    }

    public sealed partial class StylesheetManager : IStylesheetManager
    {
        [Dependency] private readonly IWispManager _wispManager = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private static ResourcePath DefaultStylesheetPath = new("/Stylesheets/Default.lua");

        private Dictionary<Type, object> _styleFallbacks = new();

        public void Initialize()
        {
            SetupStyleFallbacks();

            TryLoadStylesheet(DefaultStylesheetPath);

            _graphics.OnWindowFocused += WindowFocusedChanged;

            WatchResources();
        }

        private void SetupStyleFallbacks()
        {
            _styleFallbacks = new()
            {
                { typeof(FontSpec), new FontSpec(14, 14) },
                { typeof(StyleBox), new StyleBoxFlat(Color.HotPink) },
                { typeof(Color), Color.HotPink }
                // { typeof(IAssetInstance), _assetManager.GetAsset(Protos.FallbackAsset) }
                // { typeof(IShaderInstance), _shaderManager.GetShader(Protos.FallbackShader) }
            };
        }

        private void TryLoadStylesheet(ResourcePath luaFile)
        {
            try
            {
                var sheet = ParseStylesheet(luaFile);
                _wispManager.Stylesheet = sheet;

                Logger.InfoS("wisp.stylesheet", $"Loaded stylesheet at {luaFile}.");
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.stylesheet", $"Failed to load stylesheet: {ex}");
            }
        }

        /// <inheritdoc/>
        public T GetStyleFallback<T>()
        {
            var type = typeof(T);

            if (type.IsEnum)
            {
                return (T)Enum.GetValues(type).GetValue(0)!;
            }

            return (T)_styleFallbacks[typeof(T)];
        }
    }
}

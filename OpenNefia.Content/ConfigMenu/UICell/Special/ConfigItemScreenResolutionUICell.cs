using Microsoft.CodeAnalysis;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System.Linq;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemScreenResolutionUICell : BaseConfigMenuUICell<ConfigScreenResolutionMenuNode>
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        private static readonly FullscreenMode DefaultResolution = new(800, 600);

        private List<FullscreenMode> _resolutions = new();
        private int _currentIndex = 0;

        public ConfigItemScreenResolutionUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigScreenResolutionMenuNode data) : base(protoId, data)
        {
        }

        public override void Initialize()
        {
            var windowSettings = _graphics.GetWindowSettings();

            _resolutions = _graphics.GetFullscreenModes(windowSettings.Display)
                .Where(res => res.Width >= DefaultResolution.Width && res.Height >= DefaultResolution.Height)
                .Reverse()
                .ToList();

            var screenSize = _graphics.WindowSize;
            _currentIndex = 0;

            for (int i = 0; i < _resolutions.Count; i++)
            {
                var resolution = _resolutions[i];
                _currentIndex = i;
                if (screenSize.X <= resolution.Width && screenSize.Y <= resolution.Height)
                    break;
            }
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (_currentIndex > 0, _currentIndex < _resolutions.Count - 1);
        }

        public override void HandleChanged(int delta)
        {
            _currentIndex = Math.Clamp(_currentIndex + delta, 0, _resolutions.Count);

            var resolution = _resolutions.Count > 0
                ? _resolutions[_currentIndex]
                : DefaultResolution;

            ConfigManager.SetCVar(MenuNode.CVarWidth, resolution.Width);
            ConfigManager.SetCVar(MenuNode.CVarHeight, resolution.Height);

            _graphics.SetWindowSettings(resolution);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            var width = ConfigManager.GetCVar(MenuNode.CVarWidth);
            var height = ConfigManager.GetCVar(MenuNode.CVarHeight);

            ValueText.Text = $"{width}x{height}";
        }
    }
}

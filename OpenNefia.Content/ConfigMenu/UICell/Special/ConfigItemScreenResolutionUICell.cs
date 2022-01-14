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
        private FullscreenMode _current = DefaultResolution;
        private FullscreenMode _minResolution = DefaultResolution;
        private FullscreenMode _maxResolution = DefaultResolution;

        public ConfigItemScreenResolutionUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigScreenResolutionMenuNode data) : base(protoId, data)
        {
        }

        public override void Initialize()
        {
            var windowSettings = _graphics.GetWindowSettings();

            _resolutions.Clear();
            _resolutions.AddRange(_graphics.GetFullscreenModes(windowSettings.Display));
            _minResolution = _resolutions.First();
            _maxResolution = _resolutions.Last();
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (_current != _minResolution, _current != _maxResolution);
        }

        public override void HandleChanged(int delta)
        {
            var index = Math.Clamp(_resolutions.IndexOf(_current), 0, _resolutions.Count);

            _current = _resolutions[index];

            ConfigManager.SetCVar(MenuNode.CVarWidth, _current.Width);
            ConfigManager.SetCVar(MenuNode.CVarHeight, _current.Height);

            _graphics.SetWindowSettings(_current);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            ValueText.Text = $"{_current.Width}x{_current.Height}";
        }
    }
}

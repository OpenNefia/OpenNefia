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
    public class ConfigItemDisplayNumberUICell : BaseConfigMenuUICell<ConfigDisplayNumberMenuNode>
    {
        [Dependency] private readonly IGraphics _graphics = default!;

        private int _displayCount;
        private int _currentIndex = 0;

        public ConfigItemDisplayNumberUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigDisplayNumberMenuNode data) : base(protoId, data)
        {
        }

        public override void Initialize()
        {
            _displayCount = _graphics.GetDisplayCount();
            _currentIndex = _graphics.GetWindowSettings().Display;
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (_currentIndex > 0, _currentIndex < _displayCount - 1);
        }

        public override void HandleChanged(int delta)
        {
            _currentIndex = Math.Clamp(_currentIndex + delta, 0, _displayCount - 1);

            ConfigManager.SetCVar(MenuNode.CVar, _currentIndex);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            var displayIndex = ConfigManager.GetCVar(MenuNode.CVar);

            ValueText.Text = $"{displayIndex}: {_graphics.GetDisplayName(displayIndex)}";
        }
    }
}

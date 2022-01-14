using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ConfigMenu
{
    public static class ConfigMenuHelpers
    {

        private static readonly PrototypeId<ConfigMenuItemPrototype> DefaultConfigMenuItemID = new("Elona.MenuDefault");

        public static void ShowConfigMenu(IPrototypeManager? _prototypeManager = null, 
            IUserInterfaceManager? _uiManager = null, 
            IConfigurationManager? _config = null)
        {
            IoCManager.Resolve(ref _prototypeManager);
            IoCManager.Resolve(ref _uiManager);
            IoCManager.Resolve(ref _config);

            var defaultMenuItem = _prototypeManager.Index(DefaultConfigMenuItemID);
            if (defaultMenuItem.Node is not ConfigSubmenuMenuNode submenuNode)
            {
                throw new InvalidDataException($"Config menu item {DefaultConfigMenuItemID} must be a {nameof(ConfigSubmenuMenuNode)}!");
            }

            _uiManager.Query<ConfigMenuLayer, ConfigMenuLayer.Args>(new ConfigMenuLayer.Args(DefaultConfigMenuItemID, submenuNode));
            _config.SaveToFile();
        }
    }
}

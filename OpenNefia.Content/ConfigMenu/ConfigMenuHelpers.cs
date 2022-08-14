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

        /// <summary>
        /// Queries the default (top-level) config menu.
        /// </summary>
        public static void QueryDefaultConfigMenu(IPrototypeManager? protos = null,
            IUserInterfaceManager? uiManager = null,
            IConfigurationManager? config = null)
        {
            IoCManager.Resolve(ref protos);
            IoCManager.Resolve(ref uiManager);
            IoCManager.Resolve(ref config);

            var defaultMenuItem = protos.Index(DefaultConfigMenuItemID);
            if (defaultMenuItem.Node is not ConfigSubmenuMenuNode submenuNode)
            {
                throw new InvalidDataException($"Config menu item {DefaultConfigMenuItemID} must be a {nameof(ConfigSubmenuMenuNode)}!");
            }

            uiManager.Query<ConfigMenuLayer, ConfigMenuLayer.Args>(new ConfigMenuLayer.Args(DefaultConfigMenuItemID, submenuNode));
            config.SaveToFile();
        }
    }
}

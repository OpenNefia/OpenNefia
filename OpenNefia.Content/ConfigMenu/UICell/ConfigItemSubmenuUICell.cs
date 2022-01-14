using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;
using ConfigMenuItemProtoId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Content.ConfigMenu.ConfigMenuItemPrototype>;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemSubmenuUICell : BaseConfigMenuUICell<ConfigSubmenuMenuNode>
    {
        public ConfigItemSubmenuUICell(ConfigMenuItemProtoId protoId, ConfigSubmenuMenuNode data) : base(protoId, data)
        {
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (false, false);
        }

        public override void HandleActivated()
        {
            Sounds.Play(Sound.Ok1);
            UserInterfaceManager.Query<ConfigMenuLayer, ConfigMenuLayer.Args>(new ConfigMenuLayer.Args(ProtoId, MenuNode));
        }
    }
}

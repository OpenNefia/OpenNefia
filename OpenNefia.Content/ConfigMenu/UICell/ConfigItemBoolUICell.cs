using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemBoolUICell : BaseConfigMenuCVarUICell<ConfigBoolMenuNode, bool>
    {
        public ConfigItemBoolUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigBoolMenuNode data) : base(protoId, data)
        {
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (CurrentValue == true, CurrentValue == false);
        }

        public override void HandleChanged(int delta)
        {
            if (delta > 0)
                CurrentValue = true;
            else
                CurrentValue = false;
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            var key = CurrentValue == true ? "Yes" : "No";
            var keyRoot = "Elona.Config.Common.YesNo.Default";

            ValueText.Text = Loc.GetString($"{keyRoot}.{key}");
        }
    }
}

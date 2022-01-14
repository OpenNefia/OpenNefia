using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System.Linq;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemEnumUICell : BaseConfigMenuUICell<ConfigEnumMenuNode>
    {
        public ConfigItemEnumUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigEnumMenuNode data) : base(protoId, data)
        {
        }

        public Enum CurrentValue
        {
            get => (Enum)ConfigManager.GetCVarRaw(MenuNode.CVar);
            set => ConfigManager.SetCVarRaw(MenuNode.CVar, value);
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            var minVal = EnumHelpers.MinValue(MenuNode.EnumType);
            var maxVal = EnumHelpers.MaxValue(MenuNode.EnumType);

            return (!CurrentValue.Equals(minVal), !CurrentValue.Equals(maxVal));
        }

        public override void HandleChanged(int delta)
        {
            var values = Enum.GetValues(MenuNode.EnumType);
            var index = Math.Clamp(Array.IndexOf(values, CurrentValue) + delta, 0, values.Length);

            var rawValue = values.GetValue(index)!;

            CurrentValue = (Enum)Convert.ChangeType(rawValue, MenuNode.EnumType)!;
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            ValueText.Text = Loc.GetPrototypeString(ProtoId, $"Choices.{CurrentValue}");
        }
    }
}

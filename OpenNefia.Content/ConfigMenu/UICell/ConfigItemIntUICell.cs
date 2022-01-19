using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemIntUICell : BaseConfigMenuCVarUICell<ConfigIntMenuNode, int>
    {
        public ConfigItemIntUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigIntMenuNode data) : base(protoId, data)
        {
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (CurrentValue > MenuNode.Min, CurrentValue < MenuNode.Max);
        }

        public override void HandleChanged(int delta)
        {
            CurrentValue = Math.Clamp(CurrentValue + (delta * MenuNode.Step), MenuNode.Min, MenuNode.Max);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            ValueText.Text = CurrentValue.ToString();
        }
    }
}

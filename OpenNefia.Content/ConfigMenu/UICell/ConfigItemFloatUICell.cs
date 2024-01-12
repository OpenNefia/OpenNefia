using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.ConfigMenu.UICell
{
    public class ConfigItemFloatUICell : BaseConfigMenuCVarUICell<ConfigFloatMenuNode, float>
    {
        public ConfigItemFloatUICell(PrototypeId<ConfigMenuItemPrototype> protoId, ConfigFloatMenuNode data) : base(protoId, data)
        {
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (CurrentValue > MenuNode.Min, CurrentValue < MenuNode.Max);
        }

        public override void HandleChanged(int delta)
        {
            CurrentValue = Math.Clamp(CurrentValue + delta * MenuNode.Step, MenuNode.Min, MenuNode.Max);
        }

        public override void RefreshConfigValueDisplay()
        {
            base.RefreshConfigValueDisplay();

            ValueText.Text = CurrentValue.ToString("F2");
        }
    }
}
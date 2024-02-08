using OpenNefia.Content.ConfigMenu;
using OpenNefia.Content.ConfigMenu.UICell;
using OpenNefia.Core.Prototypes;
using OpenNefia.VisualAI.Engine;

namespace OpenNefia.VisualAI.UserInterface
{
    public class DynamicVariableIntCell : BaseDynamicVariableListCell
    {
        public DynamicVariableIntCell(IDynamicVariableItem data, int? min, int? max, int step) : base(data)
        {
            Min = min;
            Max = max;
            Step = step;
        }

        public int? Min { get; }
        public int? Max { get; }
        public int Step { get; }

        public int InnerValue
        {
            get => (int)CurrentValue!; 
            set => CurrentValue = value;
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (Min == null || InnerValue > Min, Max == null || InnerValue < Max);
        }

        public override void Change(int delta)
        {
            InnerValue += delta * Step;
            if (Min != null)
                InnerValue = int.Max(InnerValue, Min.Value);
            if (Max != null)
                InnerValue = int.Min(InnerValue, Max.Value);
        }

        public override string ValueText => InnerValue.ToString();
    }
}

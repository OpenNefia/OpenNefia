using OpenNefia.Core.Prototypes;

namespace OpenNefia.VisualAI.UserInterface
{
    public class DynamicVariableFloatCell : BaseDynamicVariableListCell
    {
        public DynamicVariableFloatCell(IDynamicVariableItem data, float? min, float? max, float step) : base(data)
        {
            Min = min;
            Max = max;
            Step = step;
        }

        public float? Min { get; }
        public float? Max { get; }
        public float Step { get; }

        private float InnerValue
        {
            get => (float)CurrentValue!; 
            set => CurrentValue = value;
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (InnerValue > Min, InnerValue < Max);
        }

        public override void Change(int delta)
        {
            InnerValue += delta * Step;
            if (Min != null)
                InnerValue = float.Max(InnerValue, Min.Value);
            if (Max != null)
                InnerValue = float.Min(InnerValue, Max.Value);
        }

        public override string ValueText => InnerValue.ToString("F2");
    }
}
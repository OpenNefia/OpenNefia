using OpenNefia.Core.Utility;

namespace OpenNefia.VisualAI.UserInterface
{
    public class DynamicVariableEnumCell : BaseDynamicVariableListCell
    {
        public DynamicVariableEnumCell(IDynamicVariableItem data) : base(data)
        {
            EnumType = data.Variable.Type;
        }

        public Type EnumType { get; }

        public Enum InnerValue
        {
            get => (Enum)Enum.ToObject(EnumType, CurrentValue!);
            set => CurrentValue = value;
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            var minVal = EnumHelpers.MinValue(EnumType);
            var maxVal = EnumHelpers.MaxValue(EnumType);

            return (!InnerValue!.Equals(minVal), !InnerValue.Equals(maxVal));
        }

        public override void Change(int delta)
        {
            var values = Enum.GetValues(EnumType);
            var index = Math.Clamp(Array.IndexOf(values, InnerValue) + delta, 0, values.Length);

            var rawValue = values.GetValue(index);
            if (rawValue != null)
                InnerValue = (Enum)Convert.ChangeType(rawValue, EnumType)!;
        }

        public override string ValueText => InnerValue.ToString();
    }
}

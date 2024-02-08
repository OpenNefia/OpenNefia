using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.VisualAI.UserInterface
{
    public class DynamicVariablePrototypeIdCell : BaseDynamicVariableListCell
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public DynamicVariablePrototypeIdCell(IDynamicVariableItem data) : base(data)
        {
            IoCManager.InjectDependencies(this);

            PrototypeType = data.Variable.Type.GetGenericArguments()[0];
            InitChoices();
        }

        private string[] _choices = new string[] { };

        private void InitChoices()
        {
            _choices = _protos.EnumeratePrototypes(PrototypeType).Select(p => p.ID).ToArray();
            if (!_choices.Contains(InnerValue))
                InnerValue = _choices.First();
        }

        public Type PrototypeType { get; }

        public string InnerValue
        {
            get => $"{CurrentValue}";
            set => CurrentValue = MakePrototypeId(value);
        }

        private int CurrentIndex => Array.IndexOf(_choices, InnerValue);

        private object? MakePrototypeId(string value)
        {
            var type = typeof(PrototypeId<>).MakeGenericType(PrototypeType);
            object[] args = { value };
            return Activator.CreateInstance(type, args);
        }

        public override (bool decArrow, bool incArrow) CanChange()
        {
            return (CurrentIndex > 0, CurrentIndex < _choices.Length);
        }

        public override void Change(int delta)
        {
            var index = Math.Clamp(CurrentIndex + delta, 0, _choices.Length);
            InnerValue = _choices[index];
        }

        public override string ValueText => InnerValue.ToString();
    }
}
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ViewVariables;

namespace OpenNefia.Core.ViewVariables.Editors
{
    internal sealed class VVPropEditorReference : VVPropEditor
    {
        [Dependency] private readonly IViewVariablesManager _viewVariables = default!;

        private object? _localValue;

        public VVPropEditorReference()
        {
            IoCManager.InjectDependencies(this);
        }

        protected override WispControl MakeUI(object? value)
        {
            if (value == null)
            {
                return new Label { Text = "null", Align = Label.AlignMode.Right };
            }

            _localValue = value;

            var toString = PrettyPrint.PrintUserFacing(value);
            var button = new Button
            {
                Text = $"Ref: {toString}",
                ClipText = true,
                HorizontalExpand = true,
            };
            button.OnPressed += ButtonOnPressed;
            return button;
        }

        private void ButtonOnPressed(BaseButton.ButtonEventArgs obj)
        {
            if (_localValue != null)
            {
                _viewVariables.OpenVV(_localValue);
            }
        }
    }
}

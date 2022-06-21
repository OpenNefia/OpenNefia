using System.Linq;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.ViewVariables;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorKeyValuePair : VVPropEditor
    {
        [Dependency] private readonly IViewVariablesManagerInternal _viewVariables = default!;

        private VVPropEditor? _propertyEditorK;
        private VVPropEditor? _propertyEditorV;

        public VVPropEditorKeyValuePair()
        {
            IoCManager.InjectDependencies(this);
        }

        protected override WispControl MakeUI(object? value)
        {
            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal
            };

            dynamic d = value!;

            // NOTE: value can be both a KeyValuePair<,> here OR as a ServerKeyValuePairToken.

            object? valueK = d.Key;
            object? valueV = d.Value;

            // ReSharper disable ConstantConditionalAccessQualifier
            var typeK = valueK?.GetType();
            var typeV = valueV?.GetType();
            // ReSharper restore ConstantConditionalAccessQualifier

            _propertyEditorK = _viewVariables.PropertyFor(typeK);
            _propertyEditorV = _viewVariables.PropertyFor(typeV);

            var controlK = _propertyEditorK.Initialize(valueK, true);
            var controlV = _propertyEditorV.Initialize(valueV, true);

            hBox.AddChild(controlK);
            hBox.AddChild(controlV);

            return hBox;
        }
    }
}

using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorIPrototype<T> : VVPropEditor
    {
        private object? _localValue;

        private ViewVariablesAddWindow? _addWindow;
        private LineEdit _lineEdit = new();

        protected override WispControl MakeUI(object? value)
        {
            _localValue = value;

            var hbox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalExpand = true
            };

            _lineEdit = new LineEdit()
            {
                HorizontalExpand = true,
                HorizontalAlignment = WispControl.HAlignment.Stretch,
                PlaceHolder = "Prototype ID",
                Text = (value as IPrototype)?.ID ?? string.Empty,
                Editable = !ReadOnly
            };

            _lineEdit.OnTextEntered += ev =>
            {
                SetNewValue(ev.Text);
            };

            var list = new Button() { Text = "List", Disabled = ReadOnly };
            var inspect = new Button() { Text = "Inspect" };

            list.OnPressed += OnListButtonPressed;
            inspect.OnPressed += OnInspectButtonPressed;

            hbox.AddChild(_lineEdit);
            hbox.AddChild(list);
            hbox.AddChild(inspect);

            return hbox;
        }

        private void OnListButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            _addWindow?.Dispose();

            WindowList();
        }

        private void WindowList()
        {
            var protoMan = IoCManager.Resolve<IPrototypeManager>();

            if (!protoMan.TryGetVariantFrom(typeof(T), out var variant)) return;

            var list = new List<string>();

            foreach (var prototype in protoMan.EnumeratePrototypes(variant))
            {
                list.Add(prototype.ID);
            }

            _addWindow = new ViewVariablesAddWindow(list, "Set Prototype");
            _addWindow.AddButtonPressed += OnAddButtonPressed;
            _addWindow.OpenCentered(_lineEdit.WispRootLayer!); // TODO
        }

        private void OnAddButtonPressed(ViewVariablesAddWindow.AddButtonPressedEventArgs obj)
        {
            _lineEdit.Text = obj.Entry;
            _addWindow?.Dispose();
            SetNewValue(obj.Entry);
        }

        private void OnInspectButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            var vvm = IoCManager.Resolve<IViewVariablesManager>();

            if (_localValue != null)
                vvm.OpenVV(_localValue);
        }

        private void SetNewValue(string text)
        {
            var protoMan = IoCManager.Resolve<IPrototypeManager>();
            if(protoMan.TryIndex(typeof(T), text, out var prototype))
                ValueChanged(prototype, false);

            return;
        }
    }
}

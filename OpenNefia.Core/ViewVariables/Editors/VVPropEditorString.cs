using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;

namespace OpenNefia.Core.ViewVariables.Editors
{
    internal sealed class VVPropEditorString : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var lineEdit = new LineEdit
            {
                Text = (string)value!,
                Editable = !ReadOnly,
                HorizontalExpand = true,
            };

            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e => ValueChanged(e.Text);
            }

            return lineEdit;
        }
    }
}

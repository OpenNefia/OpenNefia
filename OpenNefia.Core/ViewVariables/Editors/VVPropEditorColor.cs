using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorColor : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var lineEdit = new LineEdit
            {
                Text = ((Color)value!).ToHex(),
                Editable = !ReadOnly,
                HorizontalExpand = true,
                // ToolTip = "Hex color here",
                PlaceHolder = "Hex color here"
            };

            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                {
                    var val = Color.FromHex(e.Text);
                    ValueChanged(val);
                };
            }

            return lineEdit;
        }
    }
}

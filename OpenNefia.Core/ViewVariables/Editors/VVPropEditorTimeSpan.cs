using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorTimeSpan : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var ts = (TimeSpan)value!;
            var lineEdit = new LineEdit
            {
                Text = ts.ToString(),
                Editable = !ReadOnly,
                MinSize = (240, 0)
            };

            lineEdit.OnTextEntered += e =>
            {
                if (TimeSpan.TryParse(e.Text, out var span))
                    ValueChanged(span);
            };

            return lineEdit;
        }
    }
}

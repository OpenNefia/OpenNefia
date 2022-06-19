using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorAngle : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(200, 0)
            };
            var angle = (Angle)value!;
            var lineEdit = new LineEdit
            {
                Text = angle.Degrees.ToString(CultureInfo.InvariantCulture),
                Editable = !ReadOnly,
                HorizontalExpand = true
            };
            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                {
                    if (!double.TryParse(e.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
                        return;

                    ValueChanged(Angle.FromDegrees(number));
                };
            }

            hBox.AddChild(lineEdit);
            hBox.AddChild(new Label { Text = "deg" });
            return hBox;
        }
    }
}

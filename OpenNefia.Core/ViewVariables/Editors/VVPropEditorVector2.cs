using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System.Globalization;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables.Editors
{
    internal sealed class VVPropEditorVector2 : VVPropEditor
    {
        private readonly bool _intVec;

        public VVPropEditorVector2(bool intVec)
        {
            _intVec = intVec;
        }

        protected override WispControl MakeUI(object? value)
        {
            var hBoxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(240, 0),
            };

            var x = new LineEdit
            {
                Editable = !ReadOnly,
                HorizontalExpand = true,
                PlaceHolder = "X",
                // ToolTip = "X"
            };

            hBoxContainer.AddChild(x);

            var y = new LineEdit
            {
                Editable = !ReadOnly,
                HorizontalExpand = true,
                PlaceHolder = "Y",
                // ToolTip = "Y"
            };

            hBoxContainer.AddChild(y);

            if (_intVec)
            {
                var vec = (Vector2i) value!;
                x.Text = vec.X.ToString(CultureInfo.InvariantCulture);
                y.Text = vec.Y.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                var vec = (Vector2) value!;
                x.Text = vec.X.ToString(CultureInfo.InvariantCulture);
                y.Text = vec.Y.ToString(CultureInfo.InvariantCulture);
            }

            void OnEntered(LineEdit.LineEditEventArgs e)
            {
                if (_intVec)
                {
                    var xVal = int.Parse(x.Text, CultureInfo.InvariantCulture);
                    var yVal = int.Parse(y.Text, CultureInfo.InvariantCulture);

                    ValueChanged(new Vector2i(xVal, yVal));
                }
                else
                {
                    var xVal = float.Parse(x.Text, CultureInfo.InvariantCulture);
                    var yVal = float.Parse(y.Text, CultureInfo.InvariantCulture);

                    ValueChanged(new Vector2(xVal, yVal));
                }
            }

            if (!ReadOnly)
            {
                x.OnTextEntered += OnEntered;
                y.OnTextEntered += OnEntered;
            }

            return hBoxContainer;
        }
    }
}

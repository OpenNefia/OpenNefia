using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;

namespace OpenNefia.Core.ViewVariables.Editors
{
    internal sealed class VVPropEditorBoolean : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var box = new CheckBox
            {
                Pressed = (bool)value!,
                Disabled = ReadOnly,
                Text = value!.ToString()!,
                MinSize = new Vector2(70, 0)
            };
            if (!ReadOnly)
            {
                box.OnToggled += args =>
                {
                    ValueChanged(args.Pressed);
                    box.Text = args.Pressed.ToString();
                };
            }
            return box;
        }
    }
}

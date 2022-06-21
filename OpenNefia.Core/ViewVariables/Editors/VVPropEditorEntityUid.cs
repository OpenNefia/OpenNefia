using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables.Editors
{
    public sealed class VVPropEditorEntityUid : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(200, 0)
            };

            var uid = (EntityUid)value!;
            var lineEdit = new LineEdit
            {
                Text = uid.ToString(),
                Editable = !ReadOnly,
                HorizontalExpand = true,
            };
            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                    ValueChanged(EntityUid.Parse(e.Text));
            }

            var vvButton = new Button()
            {
                Text = "View",
            };

            vvButton.OnPressed += e =>
            {
                IoCManager.Resolve<IViewVariablesManager>().OpenVV(uid);
            };

            hBox.AddChild(lineEdit);
            hBox.AddChild(vvButton);
            return hBox;
        }
    }
}

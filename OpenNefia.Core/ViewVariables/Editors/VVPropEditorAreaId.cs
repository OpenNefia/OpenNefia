using OpenNefia.Core.Areas;
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
    public sealed class VVPropEditorAreaId : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(200, 0)
            };

            var areaId = (AreaId)value!;
            var lineEdit = new LineEdit
            {
                Text = areaId.ToString(),
                Editable = !ReadOnly,
                HorizontalExpand = true,
            };
            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                    ValueChanged(new AreaId(int.Parse(e.Text)));
            }

            var vvButton = new Button()
            {
                Text = "View Area",
            };

            vvButton.OnPressed += e =>
            {
                var areaMan = IoCManager.Resolve<IAreaManager>();
                if (areaMan.TryGetArea(areaId, out var area))
                    IoCManager.Resolve<IViewVariablesManager>().OpenVV(area);
            };

            hBox.AddChild(lineEdit);
            hBox.AddChild(vvButton);
            return hBox;
        }
    }
}

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
    public sealed class VVPropEditorMapId : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(200, 0)
            };

            var mapId = (MapId)value!;
            var lineEdit = new LineEdit
            {
                Text = mapId.ToString(),
                Editable = !ReadOnly,
                HorizontalExpand = true,
            };
            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                    ValueChanged(new MapId(int.Parse(e.Text)));
            }

            var vvButton = new Button()
            {
                Text = "View Map",
            };

            vvButton.OnPressed += e =>
            {
                var mapMan = IoCManager.Resolve<IMapManager>();
                if (mapMan.TryGetMap(mapId, out var map))
                    IoCManager.Resolve<IViewVariablesManager>().OpenVV(map);
            };

            hBox.AddChild(lineEdit);
            hBox.AddChild(vvButton);
            return hBox;
        }
    }
}

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
    public sealed class VVPropEditorEntityCoordinates : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            var coords = (EntityCoordinates) value!;
            var hBoxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                MinSize = new Vector2(240, 0),
            };

            hBoxContainer.AddChild(new Label {Text = "map: "});

            var entityManager = IoCManager.Resolve<IEntityManager>();

            var gridId = new LineEdit
            {
                Editable = !ReadOnly,
                HorizontalExpand = true,
                PlaceHolder = "Grid ID",
                // ToolTip = "Grid ID",
                Text = coords.GetMapId(entityManager).ToString()
            };

            hBoxContainer.AddChild(gridId);

            hBoxContainer.AddChild(new Label {Text = "pos: "});

            var x = new LineEdit
            {
                Editable = !ReadOnly,
                HorizontalExpand = true,
                PlaceHolder = "X",
                // ToolTip = "X",
                Text = coords.X.ToString(CultureInfo.InvariantCulture)
            };

            hBoxContainer.AddChild(x);

            var y = new LineEdit
            {
                Editable = !ReadOnly,
                HorizontalExpand = true,
                PlaceHolder = "Y",
                // ToolTip = "Y",
                Text = coords.Y.ToString(CultureInfo.InvariantCulture)
            };

            hBoxContainer.AddChild(y);

            void OnEntered(LineEdit.LineEditEventArgs e)
            {
                var gridVal = int.Parse(gridId.Text, CultureInfo.InvariantCulture);
                var mapManager = IoCManager.Resolve<IMapManager>();
                var xVal = int.Parse(x.Text, CultureInfo.InvariantCulture);
                var yVal = int.Parse(y.Text, CultureInfo.InvariantCulture);

                if (!mapManager.TryGetMap(new MapId(gridVal), out var map))
                {
                    ValueChanged(new EntityCoordinates(EntityUid.Invalid, (xVal, yVal)));
                    return;
                }

                ValueChanged(new EntityCoordinates(map.MapEntityUid, (xVal, yVal)));
            }

            if (!ReadOnly)
            {
                gridId.OnTextEntered += OnEntered;
                x.OnTextEntered += OnEntered;
                y.OnTextEntered += OnEntered;
            }

            return hBoxContainer;
        }
    }
}

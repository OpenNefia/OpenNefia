using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.Containers
{
    public class UiHorizontalContainer : UiContainer
    {
        public float XSpace { get; set; }

        protected override Vector2i RelayoutPreferredSize()
        {
            float yOffset = 0, xOffset = 0;
            float yMax = 0, xTotal = 0;
            foreach (var child in Entries)
            {
                switch (child.Type)
                {
                    case LayoutType.Element:
                        if (child.Element == null)
                        {
                            Logger.WarningS("ui.layout", "UiElement in container was null");
                            break;
                        }
                        child.Element.SetPreferredSize();
                        child.Element.SetPosition(X + xTotal, Y + yOffset);

                        if (child.Element.Height + yOffset > yMax)
                            yMax = child.Element.Height + yOffset;
                        xTotal += child.Element.Width + xOffset + XSpace;

                        break;

                    case LayoutType.Spacer:
                        xTotal += child.Offset;
                        break;

                    case LayoutType.XMin:
                        if (xTotal < child.Offset)
                            xTotal = child.Offset;
                        break;
                    case LayoutType.YMin:
                        if (yOffset < child.Offset)
                            yOffset = child.Offset;
                        break;

                    case LayoutType.XOffset:
                        xOffset = child.Offset;
                        break;
                    case LayoutType.YOffset:
                        yOffset = child.Offset;
                        break;

                    case LayoutType.FullUnset:
                        yOffset = 0;
                        xOffset = 0;
                        break;
                    case LayoutType.XUnset:
                        xOffset = 0;
                        break;
                    case LayoutType.YUnset:
                        yOffset = 0;
                        break;
                }
            }

            SetSize(xTotal, yMax);
            return new Vector2(xTotal, yMax);
        }
    }
}

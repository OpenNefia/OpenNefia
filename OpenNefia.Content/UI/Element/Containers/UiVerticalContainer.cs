using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.Containers
{
    public class UiVerticalContainer : UiContainer
    {
        public int YSpace { get; set; }

        protected override Vector2i RelayoutPreferredSize()
        {
            int xOffset = 0, yOffset = 0;
            int xMax = 0, yTotal = 0;
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
                        child.Element.SetPosition(X + xOffset, Y + yTotal);

                        if (child.Element.Width + xOffset > xMax)
                            xMax = child.Element.Width + xOffset;
                        yTotal += child.Element.Height + yOffset + YSpace;

                        break;

                    case LayoutType.Spacer:
                        yTotal += child.Offset;
                        break;

                    case LayoutType.YMin:
                        if (yTotal < child.Offset)
                            yTotal = child.Offset;
                        break;
                    case LayoutType.XMin:
                        if (xOffset < child.Offset)
                            xOffset = child.Offset;
                        break;

                    case LayoutType.XOffset:
                        xOffset = child.Offset;
                        break;
                    case LayoutType.YOffset:
                        yOffset = child.Offset;
                        break;

                    case LayoutType.FullUnset:
                        xOffset = 0;
                        yOffset = 0;
                        break;
                    case LayoutType.XUnset:
                        xOffset = 0;
                        break;
                    case LayoutType.YUnset:
                        yOffset = 0;
                        break;
                }
            }

            SetSize(xMax, yTotal);
            return new Vector2i(xMax, yTotal);
        }



    }
}

using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.Containers
{
    public enum GridType
    {
        Horizontal,
        Vertical,
    }
    public class UiGridContainer : UiContainer
    {
        private GridType GridType;
        /// <summary>
        /// Amount of columns when horizontal / rows when vertical
        /// </summary>
        private int Length;
        private int XSpace;
        private int YSpace;
        private bool XCentered;
        private bool YCentered;

        public UiGridContainer(GridType gridType, int length, bool xCentered = true, bool yCentered = true, int xSpace = 0, int ySpace = 0)
        {
            GridType = gridType;
            Length = length;
            XSpace = xSpace;
            YSpace = ySpace;
            XCentered = xCentered;
            YCentered = yCentered;
        }

        protected override Vector2i RelayoutPreferredSize()
        {
            int xMax = 0, yMax = 0;
            int xOffset = 0, yOffset = 0;
            int elemIndex = 0;
            foreach (var elem in Entries)
            {
                switch (elem.Type)
                {
                    case LayoutType.Element:
                        if (elem.Element == null)
                            break;
                        elem.Element.SetPreferredSize();

                        if (elem.Element.Width + XSpace > xMax)
                            xMax = elem.Element.Width + XSpace;
                        if (elem.Element.Height + YSpace > yMax)
                            yMax = elem.Element.Height + YSpace;
                        break;

                    case LayoutType.XMin:
                        if (xMax < elem.Offset)
                            xMax = elem.Offset + XSpace;
                        break;
                    case LayoutType.YMin:
                        if (yMax < elem.Offset)
                            yMax = elem.Offset + YSpace;
                        break;
                }
            }
            foreach (var elem in Entries)
            {
                switch (elem.Type)
                {
                    case LayoutType.Spacer:
                    case LayoutType.Element:
                        if (elem.Element == null)
                        {
                            elemIndex++;
                            break;
                        }

                        int mainOffset = elemIndex % Length;
                        int subOffset;
                        switch(GridType)
                        {
                            case GridType.Horizontal:
                                mainOffset *= xMax;
                                mainOffset += xOffset;
                                subOffset = (yMax * (elemIndex / Length)) + yOffset;

                                if (XCentered)
                                    mainOffset += ((xMax - elem.Element.Width) / 2);

                                if (YCentered)
                                    subOffset += ((yMax - elem.Element.Height) / 2);

                                elem.Element.SetPosition(X + mainOffset, Y + subOffset);
                                break;
                            case GridType.Vertical:
                                mainOffset *= yMax;
                                mainOffset += yOffset;
                                subOffset = (xMax * (elemIndex / Length)) + xOffset;

                                if (XCentered)
                                    subOffset += ((xMax - elem.Element.Width) / 2);

                                if (YCentered)
                                    mainOffset += ((yMax - elem.Element.Height) / 2);

                                elem.Element.SetPosition(X + subOffset, Y + mainOffset);
                                break;
                        }
                        elemIndex++;
                        break;
                }
            }
            int totalElements = (elemIndex + 1);
            switch (GridType)
            {
                default:
                case GridType.Horizontal:
                    return new Vector2i(Math.Min(totalElements, Length) * xMax, (totalElements / Length) * yMax);
                case GridType.Vertical:
                    return new Vector2i((totalElements / Length) * xMax, Math.Min(totalElements, Length) * yMax);
            }
        }
    }
}

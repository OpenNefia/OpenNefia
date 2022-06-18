using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Wisp.CustomControls
{
    /// <summary>
    /// Controls how a window will be positioned when it is first opened.
    /// </summary>
    public enum WindowAnchor
    {
        Center = 0b00000,
        Left = 0b00001,
        Top = 0b00010,
        Right = 0b00100,
        Bottom = 0b01000,

        TopLeft = Left | Top,
        TopRight = Right | Top,
        BottomLeft = Left | Bottom,
        BottomRight = Right | Bottom,
    }
}

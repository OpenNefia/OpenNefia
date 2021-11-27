using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public struct MouseButtonPress
    {
        public MouseButtons Button;
        public int X;
        public int Y;

        public MouseButtonPress(MouseButtons button, int x, int y)
        {
            this.Button = button;
            this.X = x;
            this.Y = y;
        }
    }
}

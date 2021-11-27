using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public class MouseMovedEvent : IInputEvent
    {
        public int X { get; }
        public int Y { get; }
        public int Dx { get; }
        public int Dy { get; }
        public bool Passed { get; private set; }

        public MouseMovedEvent(int x, int y, int dx, int dy)
        {
            this.X = x;
            this.Y = y;
            this.Dx = dx;
            this.Dy = dy;
            this.Passed = false;
        }

        public void Pass() { this.Passed = true; }
    }
}

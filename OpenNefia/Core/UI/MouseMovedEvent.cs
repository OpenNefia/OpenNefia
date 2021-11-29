using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public class MouseMovedEvent : IInputEvent
    {
        public Vector2i Pos;
        public Vector2i DPos;
        public bool Passed { get; private set; }

        public MouseMovedEvent(Vector2i pos, Vector2i dpos)
        {
            this.Pos = pos;
            this.DPos = dpos;
            this.Passed = false;
        }

        public void Pass() { this.Passed = true; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public class KeyInputEvent : IInputEvent
    {
        public KeyPressState State { get; }
        public bool Passed { get; private set; }

        public KeyInputEvent(KeyPressState state)
        {
            this.State = state;
            this.Passed = false;
        }

        public void Pass() { this.Passed = true; }
    }
}

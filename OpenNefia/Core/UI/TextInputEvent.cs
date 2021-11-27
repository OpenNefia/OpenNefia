using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public class TextInputEvent: IInputEvent
    {
        public string Text { get; }
        public bool Passed { get; private set; }

        public TextInputEvent(string text)
        {
            this.Text = text;
            this.Passed = false;
        }

        public void Pass() { this.Passed = true; }
    }
}

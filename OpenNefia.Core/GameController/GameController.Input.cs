using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameController
{
    public sealed partial class GameController
    {
        private void KeyUp(KeyEventArgs args)
        {
            _inputManager.KeyUp(args);
        }

        private void KeyDown(KeyEventArgs args)
        {
            _inputManager.KeyDown(args);
        }

        private void TextEditing(TextEditingEventArgs args)
        {
        }

        private void MouseMoved(MouseMoveEventArgs args)
        {
        }

        private void MouseWheel(MouseWheelEventArgs args)
        {
        }
    }
}

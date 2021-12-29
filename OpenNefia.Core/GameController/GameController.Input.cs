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
            _uiManager.MouseMove(args);
        }

        private void MousePressed(MouseButtonEventArgs args)
        {
            var key = Mouse.MouseButtonToKey(args.Button);
            var ev = new KeyEventArgs(key, false, false, false, false, false, Love.Scancode.Unknow);
            _inputManager.KeyDown(ev);
        }

        private void MouseReleased(MouseButtonEventArgs args)
        {
            var key = Mouse.MouseButtonToKey(args.Button);
            var ev = new KeyEventArgs(key, false, false, false, false, false, Love.Scancode.Unknow);
            _inputManager.KeyUp(ev);
        }

        private void MouseWheel(MouseWheelEventArgs args)
        {
            _uiManager.MouseWheel(args);
        }
    }
}

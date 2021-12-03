using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public abstract class UiInputEventArgs : EventArgs
    {
        public bool Passed { get; private set; }

        public void Pass()
        {
            Passed = true;
        }
    }

    public sealed class UiTextInputEventArgs : UiInputEventArgs
    {
        public UiTextInputEventArgs(string text)
        {
            this.Text = text;
        }

        public string Text { get; }
    }

    public sealed class UiKeyInputEventArgs : UiInputEventArgs
    {
        public UiKeyInputEventArgs(IKeybind keybind, KeyPressState state)
        {
            Keybind = keybind;
            State = state;
        }

        public IKeybind Keybind { get; }
        public KeyPressState State { get; }
    }

    public sealed class UiMouseMovedEventArgs : UiInputEventArgs
    {
        public UiMouseMovedEventArgs(Vector2i pos, Vector2i dpos)
        {
            Pos = pos;
            DPos = dpos;
        }

        public Vector2i Pos { get; }
        public Vector2i DPos { get; }
    }

    public sealed class UiMousePressedEventArgs : UiInputEventArgs
    {
        public UiMousePressedEventArgs(MouseButton button, KeyPressState state, Vector2i pos)
        {
            Button = button;
            State = state;
            Pos = pos;
        }

        public MouseButton Button { get; }
        public KeyPressState State { get; }
        public Vector2i Pos { get; }
    }
}

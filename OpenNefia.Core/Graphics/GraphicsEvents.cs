using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public readonly struct WindowFocusedEventArgs
    {
        public WindowFocusedEventArgs(bool focused)
        {
            Focused = focused;
        }

        public bool Focused { get; }
    }

    public readonly struct WindowResizedEventArgs
    {
        public WindowResizedEventArgs(Vector2i newSize)
        {
            NewSize = newSize;
        }

        public Vector2i NewSize { get; }
    }

    public readonly struct KeyPressedEventArgs
    {
        public KeyPressedEventArgs(KeyConstant key, Scancode scancode, bool isRepeat, bool isPressed)
        {
            Key = key;
            Scancode = scancode;
            IsRepeat = isRepeat;
            IsPressed = isPressed;
        }

        public KeyConstant Key { get; }
        public Scancode Scancode { get; }
        public bool IsRepeat { get; }
        public bool IsPressed { get; }
    }

    public readonly struct TextEditingEventArgs
    {
        public TextEditingEventArgs(string text, int start, int end)
        {
            Text = text;
            Start = start;
            End = end;
        }

        public string Text { get; }
        public int Start { get; }
        public int End { get; }
    }

    public readonly struct TextInputEventArgs
    {
        public TextInputEventArgs(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    public readonly struct MouseMovedEventArgs
    {
        public MouseMovedEventArgs(Vector2 pos, Vector2 dpos, bool isTouch)
        {
            Pos = pos;
            DPos = dpos;
            IsTouch = isTouch;
        }

        public Vector2 Pos { get; }
        public Vector2 DPos { get; }
        public bool IsTouch { get; }
    }

    public readonly struct MousePressedEventArgs
    {
        public MousePressedEventArgs(Vector2 pos, UI.MouseButton button, bool isTouch, bool isPressed)
        {
            Pos = pos;
            Button = button;
            IsTouch = isTouch;
            IsPressed = isPressed;
        }

        public Vector2 Pos { get; }
        public UI.MouseButton Button { get; }
        public bool IsTouch { get; }
        public bool IsPressed { get; }
    }
}

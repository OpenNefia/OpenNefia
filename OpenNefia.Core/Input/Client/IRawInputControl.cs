using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using Scancode = Love.Scancode;

namespace OpenNefia.Core.Input
{
    /// <summary>
    /// Allows a control to listen for raw keyboard events. This allows bypassing the input binding system.
    /// </summary>
    /// <remarks>
    /// Raw key events are raised *after* keybindings and focusing has been calculated,
    /// but before key bind events are actually raised.
    /// This is necessary to allow UI system stuff to actually work correctly.
    /// </remarks>
    public interface IRawInputControl : IUiElement
    {
        /// <param name="guiRawEvent"></param>
        /// <returns>If true: all further key bind events should be blocked.</returns>
        bool RawKeyEvent(in GuiRawKeyEvent guiRawEvent) => false;
        // bool RawCharEvent(in GuiRawCharEvent guiRawCharEvent) => false;
    }

    /*
    internal struct GuiRawCharEvent
    {
        // public readonly
        public readonly RawKeyAction Action;
        public readonly Vector2i MouseRelative;
        public readonly Rune Char;
    }
    */

    public readonly struct GuiRawKeyEvent
    {
        public readonly Keyboard.Key Key;
        public readonly Scancode ScanCode;
        public readonly RawKeyAction Action;
        public readonly Vector2i MouseRelative;

        public GuiRawKeyEvent(Keyboard.Key key, Scancode scanCode, RawKeyAction action, Vector2i mouseRelative)
        {
            Key = key;
            ScanCode = scanCode;
            Action = action;
            MouseRelative = mouseRelative;
        }
    }

    public enum RawKeyAction : byte
    {
        Down,
        Repeat,
        Up
    }
}

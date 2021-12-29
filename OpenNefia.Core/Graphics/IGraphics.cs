using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Graphics
{
    public interface IGraphics
    {
        Vector2i WindowSize { get; }

        event Action<WindowResizedEventArgs>? OnWindowResized;
        event Action<WindowFocusedEventArgs>? OnWindowFocused;
        event Action<KeyEventArgs>? OnKeyPressed;
        event Action<KeyEventArgs>? OnKeyReleased;
        event Action<TextEditingEventArgs>? OnTextEditing;
        event Action<TextEventArgs>? OnTextInput;
        event Action<MouseMoveEventArgs>? OnMouseMoved;
        event Action<MouseButtonEventArgs>? OnMousePressed;
        event Action<MouseButtonEventArgs>? OnMouseReleased;
        event Action<MouseWheelEventArgs>? OnMouseWheel;
        event Func<QuitEventArgs, bool>? OnQuit;

        void Initialize();
        void Shutdown();
        void ShowSplashScreen();

        void BeginDraw();
        void EndDraw();
    }
}

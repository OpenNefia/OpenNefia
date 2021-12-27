using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Graphics
{
    public interface IGraphics
    {
        event Action<WindowResizedEventArgs>? OnWindowResized;
        event Action<WindowFocusedEventArgs>? OnWindowFocused;
        event Action<KeyPressedEventArgs>? OnKeyPressed;
        event Action<KeyPressedEventArgs>? OnKeyReleased;
        event Action<TextEditingEventArgs>? OnTextEditing;
        event Action<TextInputEventArgs>? OnTextInput;
        event Action<MouseMovedEventArgs>? OnMouseMoved;
        event Action<MousePressedEventArgs>? OnMousePressed;
        event Action<MousePressedEventArgs>? OnMouseReleased;

        void Initialize();
        void Shutdown();
        void ShowSplashScreen();

        void BeginDraw();
        void EndDraw();
    }
}

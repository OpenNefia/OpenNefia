using Love;
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

        WindowSettings GetWindowSettings();
        void SetWindowSettings(FullscreenMode mode, WindowSettings? windowSettings = null);
        IEnumerable<FullscreenMode> GetFullscreenModes(int displayIndex);
        int GetDisplayCount();
        string GetDisplayName(int displayIndex);

        void Initialize();
        void Shutdown();
        void ShowSplashScreen();

        void BeginDraw();
        void EndDraw();
    }

    public record struct FullscreenMode(int Width, int Height)
    {
        public static implicit operator Vector2i(FullscreenMode mode)
        {
            return new(mode.Width, mode.Height);
        }

        public static implicit operator FullscreenMode(Vector2i vector)
        {
            return new(vector.X, vector.Y);
        }
    }
}

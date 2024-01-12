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
        /// <summary>
        /// Window scale.
        /// </summary>
        float WindowScale { get; }

        /// <summary>
        /// Window size in virtual pixels, according to global UI scaling settings.
        /// </summary>
        Vector2 WindowSize { get; }

        /// <summary>
        /// Window size in physical pixels.
        /// </summary>
        Vector2i WindowPixelSize { get; }

        event Action<WindowResizedEventArgs>? OnWindowResized;
        event Action<WindowFocusChangedEventArgs>? OnWindowFocusChanged;
        event Action<KeyEventArgs>? OnKeyPressed;
        event Action<KeyEventArgs>? OnKeyReleased;
        event Action<TextEditingEventArgs>? OnTextEditing;
        event Action<TextEventArgs>? OnTextInput;
        event Action<MouseMoveEventArgs>? OnMouseMoved;
        event Action<MouseButtonEventArgs>? OnMousePressed;
        event Action<MouseButtonEventArgs>? OnMouseReleased;
        event Action<MouseWheelEventArgs>? OnMouseWheel;
        event Action<WindowScaleChangedEventArgs>? OnWindowScaleChanged;
        event Action<FontHeightScaleChangedEventArgs>? OnFontHeightScaleChanged;
        event Func<QuitEventArgs, bool>? OnQuit;

        WindowSettings GetWindowSettings();
        void SetWindowSettings(FullscreenMode mode, WindowSettings? windowSettings = null);
        IEnumerable<FullscreenMode> GetFullscreenModes(int displayIndex);
        int GetDisplayCount();
        string GetDisplayName(int displayIndex);
        void SetCursor(CursorShape cursorShape);

        Love.ImageData? CaptureCanvasImageData();

        /// <summary>
        /// Captures the current state of the rendering canvas as
        /// the bytes of a PNG file.
        /// </summary>
        byte[] CaptureCanvasPNG();

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

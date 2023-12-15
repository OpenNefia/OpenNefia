using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public class HeadlessGraphics : Love.Scene, IGraphics, IClipboardManager
    {
        public float WindowScale => 1f;
        public Vector2 WindowSize => (Vector2)WindowPixelSize * WindowScale;
        public Vector2i WindowPixelSize => new(800, 600);

        public event Action<WindowResizedEventArgs>? OnWindowResized;
        public new event Action<WindowFocusChangedEventArgs>? OnWindowFocusChanged;
        public new event Action<KeyEventArgs>? OnKeyPressed;
        public new event Action<KeyEventArgs>? OnKeyReleased;
        public new event Action<TextEditingEventArgs>? OnTextEditing;
        public new event Action<TextEventArgs>? OnTextInput;
        public new event Action<MouseMoveEventArgs>? OnMouseMoved;
        public new event Action<MouseButtonEventArgs>? OnMousePressed;
        public new event Action<MouseButtonEventArgs>? OnMouseReleased;
        public new event Action<MouseWheelEventArgs>? OnMouseWheel;
        public event Action<WindowScaleChangedEventArgs>? OnWindowScaleChanged;
        public new event Func<QuitEventArgs, bool>? OnQuit;

        public void Initialize()
        {
            // TODO: Much of Love.Graphics requires the window to be initialized
            // and visible.
            var bootConfig = new Love.BootConfig()
            {
                WindowTitle = Engine.Title,
                WindowDisplay = 0,
                WindowMinWidth = 800,
                WindowMinHeight = 600,
                WindowVsync = true,
                WindowResizable = true,
                DefaultRandomSeed = 0
            };

            Love.Boot.Init(bootConfig);
            Love.Timer.Step();
            Love.Boot.SystemStep(this);
        }

        public void Shutdown()
        {
        }

        public WindowSettings GetWindowSettings()
        {
            return new();
        }

        public void SetWindowSettings(FullscreenMode mode, WindowSettings? windowSettings = null)
        {
        }

        public IEnumerable<FullscreenMode> GetFullscreenModes(int displayIndex)
        {
            return Enumerable.Empty<FullscreenMode>();
        }

        public int GetDisplayCount()
        {
            return 1;
        }

        public string GetDisplayName(int displayIndex)
        {
            return string.Empty;
        }

        public void SetCursor(CursorShape cursorShape)
        {
        }

        string IClipboardManager.GetText()
        {
            return string.Empty;
        }

        void IClipboardManager.SetText(string text)
        {
        }

        public void ShowSplashScreen()
        {
        }

        public void BeginDraw()
        {
        }

        public void EndDraw()
        {
        }

        public byte[] CaptureCanvasPNG()
        {
            return Array.Empty<byte>();
        }
    }
}
using Love;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UI;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public class HeadlessGraphics : Love.Scene, IGraphics
    {
        public Vector2i WindowSize => new(800, 600);

        public event Action<WindowResizedEventArgs>? OnWindowResized;
        public new event Action<WindowFocusedEventArgs>? OnWindowFocused;
        public new event Action<KeyEventArgs>? OnKeyPressed;
        public new event Action<KeyEventArgs>? OnKeyReleased;
        public new event Action<TextEditingEventArgs>? OnTextEditing;
        public new event Action<TextEventArgs>? OnTextInput;
        public new event Action<MouseMoveEventArgs>? OnMouseMoved;
        public new event Action<MouseButtonEventArgs>? OnMousePressed;
        public new event Action<MouseButtonEventArgs>? OnMouseReleased;
        public new event Action<MouseWheelEventArgs>? OnMouseWheel;
        public new event Func<QuitEventArgs, bool>? OnQuit;

        public void Initialize()
        {
            // TODO: Much of Love.Graphics requires the window to be initialized
            // and visible.
            var bootConfig = new BootConfig()
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

        public void ShowSplashScreen()
        {
        }

        public void BeginDraw()
        {
        }

        public void EndDraw()
        {
        }
    }
}
using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;

namespace OpenNefia.Core.Graphics
{
    public class LoveGraphics : Love.Scene, IGraphics
    {
        public event Action<WindowResizedEventArgs>? OnWindowResized;
        public event Action<WindowFocusedEventArgs>? OnWindowFocused;
        public new event Action<KeyPressedEventArgs>? OnKeyPressed;
        public new event Action<KeyPressedEventArgs>? OnKeyReleased;
        public new event Action<TextEditingEventArgs>? OnTextEditing;
        public new event Action<TextInputEventArgs>? OnTextInput;
        public new event Action<MouseMovedEventArgs>? OnMouseMoved;
        public new event Action<MousePressedEventArgs>? OnMousePressed;
        public new event Action<MousePressedEventArgs>? OnMouseReleased;

        private Love.Canvas TargetCanvas = default!;

        public void Initialize()
        {
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

            var iconData = Love.Image.NewImageData("Assets/Core/Icon/icon.png");
            Love.Window.SetIcon(iconData);

            Love.Boot.SystemStep(this);

            TargetCanvas = Love.Graphics.NewCanvas(Love.Graphics.GetWidth(), Love.Graphics.GetHeight());

            OnWindowResized += HandleWindowResized;

            InitializeGraphicsDefaults();
        }

        public void Shutdown()
        {
            OnWindowResized -= HandleWindowResized;
        }

        private void InitializeGraphicsDefaults()
        {
            Love.Graphics.SetLineStyle(Love.LineStyle.Rough);
            Love.Graphics.SetLineWidth(1);
        }

        public void BeginDraw()
        {
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetCanvas(TargetCanvas);
            Love.Graphics.Clear();
        }

        public void EndDraw()
        {
            Love.Graphics.SetCanvas();
            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha, Love.BlendAlphaMode.PreMultiplied);

            Love.Graphics.Draw(TargetCanvas);
            Love.Graphics.SetColor(1f, 0f, 1f, 1f);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, 1, 1, 1100, 100);
        }

        #region Love Event Handlers

        public void HandleWindowResized(WindowResizedEventArgs args)
        {
            TargetCanvas = Love.Graphics.NewCanvas(args.NewSize.X, args.NewSize.Y);
        }

        #endregion

        #region Love Events

        public override void WindowResize(int w, int h)
        {
            OnWindowResized?.Invoke(new WindowResizedEventArgs(new Vector2i(w, h)));
        }

        public override void WindowFocus(bool focus)
        {
            OnWindowFocused?.Invoke(new WindowFocusedEventArgs(focus));
        }

        public override void KeyPressed(Love.KeyConstant key, Love.Scancode scancode, bool isRepeat)
        {
            OnKeyPressed?.Invoke(new KeyPressedEventArgs(key, scancode, isRepeat, true));
        }

        public override void KeyReleased(Love.KeyConstant key, Love.Scancode scancode)
        {
            OnKeyReleased?.Invoke(new KeyPressedEventArgs(key, scancode, false, false));
        }

        public override void TextEditing(string text, int start, int end)
        {
            OnTextEditing?.Invoke(new TextEditingEventArgs(text, start, end));
        }

        public override void TextInput(string text)
        {
            OnTextInput?.Invoke(new TextInputEventArgs(text));
        }

        public override void MouseMoved(float x, float y, float dx, float dy, bool isTouch)
        {
            OnMouseMoved?.Invoke(new MouseMovedEventArgs(new Vector2(x, y), new Vector2(dx, dy), isTouch));
        }

        public override void MousePressed(float x, float y, int button, bool isTouch)
        {
            OnMousePressed?.Invoke(new MousePressedEventArgs(new Vector2(x, y), (MouseButtons)button, isTouch, true));
        }

        public override void MouseReleased(float x, float y, int button, bool isTouch)
        {
            OnMouseReleased?.Invoke(new MousePressedEventArgs(new Vector2(x, y), (MouseButtons)button, isTouch, false));
        }

        #endregion
    }
}

using Love;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UI;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public class LoveGraphics : Love.Scene, IGraphics
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;

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

        private Love.Canvas TargetCanvas = default!;

        private int _modControl;
        private int _modAlt;
        private int _modShift;
        private int _modSystem;

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

            var data = _resourceCache.GetResource<LoveFileDataResource>("/Icon/Core/icon.png");
            var iconData = Love.Image.NewImageData(data);
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

        public void ShowSplashScreen()
        {
            // TODO
            BeginDraw();

            var text = Love.Graphics.NewText(new FontSpec(20, 20).LoveFont, "Now Loading...");
            var x = Love.Graphics.GetWidth() / 2;
            var y = Love.Graphics.GetHeight() / 2;
            Love.Graphics.Clear(Love.Color.Black);
            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.Draw(text, x - text.GetWidth() / 2, y - text.GetHeight() / 2);

            EndDraw();

            Love.Graphics.Present();
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

        private bool PressModifiers(Love.KeyConstant key)
        {
            switch (key)
            {
                case KeyConstant.LCtrl:
                case KeyConstant.RCtrl:
                    _modControl++;
                    return true;
                case KeyConstant.LAlt:
                case KeyConstant.RAlt:
                    _modAlt++;
                    return true;
                case KeyConstant.LShift:
                case KeyConstant.RShift:
                    _modShift++;
                    return true;
                case KeyConstant.LGUI:
                case KeyConstant.RGUI:
                    _modSystem++;
                    return true;
                default:
                    return false;
            }
        }

        private bool ReleaseModifiers(Love.KeyConstant key)
        {
            switch (key)
            {
                case KeyConstant.LCtrl:
                case KeyConstant.RCtrl:
                    _modControl--;
                    return true;
                case KeyConstant.LAlt:
                case KeyConstant.RAlt:
                    _modAlt--;
                    return true;
                case KeyConstant.LShift:
                case KeyConstant.RShift:
                    _modShift--;
                    return true;
                case KeyConstant.LGUI:
                case KeyConstant.RGUI:
                    _modSystem--;
                    return true;
                default:
                    return false;
            }
        }

        public override void KeyPressed(Love.KeyConstant key, Love.Scancode scancode, bool isRepeat)
        {
            var shift = false;
            var alt = false;
            var control = false;
            var system = false;

            if (!PressModifiers(key)) 
            {
                shift = _modShift != 0;
                alt = _modAlt != 0;
                control = _modControl != 0;
                system = _modSystem != 0;
            }

            var ev = new KeyEventArgs(
                (Input.Keyboard.Key)key,
                isRepeat,
                alt, control, shift, system,
                scancode);

            OnKeyPressed?.Invoke(ev);
        }

        public override void KeyReleased(Love.KeyConstant key, Love.Scancode scancode)
        {
            var shift = false;
            var alt = false;
            var control = false;
            var system = false;

            if (!ReleaseModifiers(key))
            {
                shift = _modShift != 0;
                alt = _modAlt != 0;
                control = _modControl != 0;
                system = _modSystem != 0;
            }

            var ev = new KeyEventArgs(
                (Input.Keyboard.Key)key,
                false,
                alt, control, shift, system,
                scancode);

            OnKeyReleased?.Invoke(ev);
        }

        public override void TextEditing(string text, int start, int end)
        {
            OnTextEditing?.Invoke(new TextEditingEventArgs(text, start, end));
        }

        public override void TextInput(string text)
        {
            OnTextInput?.Invoke(new TextEventArgs(text));
        }

        public override void MouseMoved(float x, float y, float dx, float dy, bool isTouch)
        {
            OnMouseMoved?.Invoke(new MouseMoveEventArgs(new ScreenCoordinates(x, y), new Vector2(dx, dy), isTouch));
        }

        public override void MousePressed(float x, float y, int button, bool isTouch)
        {
            OnMousePressed?.Invoke(new MouseButtonEventArgs(new ScreenCoordinates(x, y), (Input.Mouse.Button)button, isTouch));
        }

        public override void MouseReleased(float x, float y, int button, bool isTouch)
        {
            OnMouseReleased?.Invoke(new MouseButtonEventArgs(new ScreenCoordinates(x, y), (Input.Mouse.Button)button, isTouch));
        }

        public override void WheelMoved(int x, int y)
        {
            OnMouseWheel?.Invoke(new MouseWheelEventArgs(new(Love.Mouse.GetPosition()), new Vector2i(x, y)));
        }

        public override bool Quit()
        {
            return OnQuit?.Invoke(new QuitEventArgs()) ?? false;
        }

        #endregion
    }
}

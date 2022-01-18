using Love;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System.Text.Unicode;
using static OpenNefia.Core.Input.Mouse;
using Vector2 = OpenNefia.Core.Maths.Vector2;

namespace OpenNefia.Core.Graphics
{
    public class LoveGraphics : Love.Scene, IGraphics
    {
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private const int MinWidth = 800;
        private const int MinHeight = 600;

        public float WindowScale { get; internal set; } = 1f;
        public Vector2i WindowPixelSize => new(Love.Graphics.GetWidth(), Love.Graphics.GetHeight());
        public Vector2 WindowSize => (Vector2)WindowPixelSize / WindowScale;

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

        private FullscreenMode _lastFullscreenMode;

        public void Initialize()
        {
            WindowScale = _config.GetCVar(CVars.DisplayUIScale);

            var bootConfig = new BootConfig()
            {
                WindowTitle = _config.GetCVar(CVars.DisplayTitle),
                WindowDisplay = _config.GetCVar(CVars.DisplayDisplayNumber),
                WindowMinWidth = Math.Max(MinWidth, (int)(MinWidth * WindowScale)),
                WindowMinHeight = Math.Max(MinHeight, (int)(MinHeight * WindowScale)),
                WindowWidth = _config.GetCVar(CVars.DisplayWidth),
                WindowHeight = _config.GetCVar(CVars.DisplayHeight),
                WindowVsync = _config.GetCVar(CVars.DisplayVSync),
                WindowHighdpi = _config.GetCVar(CVars.DisplayHighDPI),
                WindowResizable = true,
                DefaultRandomSeed = 0
            };

            switch (_config.GetCVar(CVars.DisplayWindowMode))
            {
                case WindowMode.Fullscreen:
                    bootConfig.WindowFullscreen = true;
                    bootConfig.WindowFullscreenType = Love.FullscreenType.Exclusive;
                    break;
                case WindowMode.DesktopFullscreen:
                    bootConfig.WindowFullscreen = true;
                    bootConfig.WindowFullscreenType = Love.FullscreenType.DeskTop;
                    break;
                case WindowMode.Windowed:
                default:
                    bootConfig.WindowFullscreen = false;
                    break;
            }

            Love.Boot.Init(bootConfig);
            Love.Timer.Step();

            var data = _resourceCache.GetResource<LoveFileDataResource>("/Icon/Core/icon.png");
            var iconData = Love.Image.NewImageData(data);
            Love.Window.SetIcon(iconData);

            Love.Boot.SystemStep(this);

            TargetCanvas = Love.Graphics.NewCanvas(WindowPixelSize.X, WindowPixelSize.Y);

            OnWindowResized += HandleWindowResized;

            _config.OnValueChanged(CVars.DisplayWindowMode, OnConfigWindowModeChanged);
            _config.OnValueChanged(CVars.DisplayDisplayNumber, OnConfigDisplayNumberChanged);

            InitializeGraphicsDefaults();
            LoadGamepadMappings();

            _lastFullscreenMode = new FullscreenMode(WindowPixelSize.X, WindowPixelSize.Y);
        }

        private void OnConfigWindowModeChanged(WindowMode obj)
        {
            var settings = GetWindowSettings();

            switch (_config.GetCVar(CVars.DisplayWindowMode))
            {
                case WindowMode.Fullscreen:
                    settings.Fullscreen = true;
                    settings.FullscreenType = FullscreenType.Exclusive;
                    break;
                case WindowMode.DesktopFullscreen:
                    settings.Fullscreen = true;
                    settings.FullscreenType = FullscreenType.Desktop;
                    break;
                case WindowMode.Windowed:
                default:
                    settings.Fullscreen = false;
                    break;
            }

            SetWindowSettings(_lastFullscreenMode, settings);
        }

        private void OnConfigDisplayNumberChanged(int displaynumber)
        {
            var settings = GetWindowSettings();

            settings.Display = displaynumber;
            
            SetWindowSettings(_lastFullscreenMode, settings);
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

        #region Windowing

        public WindowSettings GetWindowSettings()
        {
            var loveWindowSettings = Love.Window.GetMode();

            // We shouldn't depend on Love2dCS's classes, in order to be backend-agnostic.
            return new WindowSettings()
            {
                Borderless = loveWindowSettings.Borderless,
                Centered = loveWindowSettings.Centered,
                Depth = loveWindowSettings.Depth,
                Display = loveWindowSettings.Display,
                Fullscreen = loveWindowSettings.Fullscreen,
                FullscreenType = (FullscreenType)loveWindowSettings.FullscreenType,
                HighDPI = loveWindowSettings.HighDpi,
                MinHeight = loveWindowSettings.MinHeight,
                MinWidth = loveWindowSettings.MinWidth,
                MSAA = loveWindowSettings.MSAA,
                RefreshRate = loveWindowSettings.Refreshrate,
                Resizable = loveWindowSettings.Resizable,
                Stencil = loveWindowSettings.Stencil,
                VSync = loveWindowSettings.Vsync,
                X = loveWindowSettings.x,
                Y = loveWindowSettings.y,
            };
        }

        public void SetWindowSettings(FullscreenMode mode, WindowSettings? windowSettings = null)
        {
            Love.WindowSettings? loveWindowSettings = null;

            var isFullscreen = Love.Window.GetFullscreen();

            if (windowSettings != null)
            {
                loveWindowSettings = new Love.WindowSettings()
                {
                    Borderless = windowSettings.Borderless,
                    Centered = windowSettings.Centered,
                    Depth = windowSettings.Depth,
                    Display = windowSettings.Display,
                    Fullscreen = windowSettings.Fullscreen,
                    FullscreenType = (Love.FullscreenType)windowSettings.FullscreenType,
                    HighDpi = windowSettings.HighDPI,
                    MinWidth = 800,
                    MinHeight = 600,
                    MSAA = windowSettings.MSAA,
                    Refreshrate = windowSettings.RefreshRate,
                    Resizable = true,
                    Stencil = windowSettings.Stencil,
                    Vsync = windowSettings.VSync,
                    x = isFullscreen ? null : windowSettings.X,
                    y = isFullscreen ? null : windowSettings.Y,
                };
            }


            Love.Window.SetMode(mode.Width, mode.Height, loveWindowSettings);

            var ev = new WindowResizedEventArgs(WindowPixelSize);
            OnWindowResized?.Invoke(ev);
        }

        public int GetDisplayCount()
        {
            return Love.Window.GetDisplayCount();
        }

        public string GetDisplayName(int displaynumber)
        {
            return Love.Window.GetDisplayName(displaynumber);
        }

        public IEnumerable<FullscreenMode> GetFullscreenModes(int displaynumber)
        {
            return Love.Window.GetFullscreenModes(displaynumber)
                .Select(point => new FullscreenMode(point.X, point.Y));
        }

        #endregion

        private void InitializeGraphicsDefaults()
        {
            Love.Graphics.SetLineStyle(Love.LineStyle.Rough);
            Love.Graphics.SetLineWidth(1);
            Love.Graphics.SetBackgroundColor(0f, 0f, 0f, 1f);
        }

        private void LoadGamepadMappings()
        {
            var mappingsPath = new ResourcePath("/Config/Core/gamecontrollerdb.txt");
            var mappings = _resourceCache.ContentFileReadAllText(mappingsPath);
            var mappingsBytes = EncodingHelpers.UTF8.GetBytes(mappings);
            Love.Joystick.LoadGamepadMappings(mappingsBytes);
        }

        public void BeginDraw()
        {
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetCanvas(TargetCanvas);
            Love.Graphics.Clear(0f, 0f, 0f, 1f);
        }

        public void EndDraw()
        {
            Love.Graphics.SetCanvas();
            Love.Graphics.SetColor(Love.Color.White);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha, Love.BlendAlphaMode.PreMultiplied);

            Love.Graphics.Draw(TargetCanvas);
        }

        public byte[] CaptureCanvasPNG()
        {
            // This emulates LÖVE 11's love.graphics.captureScreenshot.

            var imageData = TargetCanvas.NewImageData();
            var fileData = imageData.Encode(ImageFormat.PNG);

            var bytes = fileData.GetBytes();

            imageData.Dispose();
            fileData.Dispose();

            return bytes;
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

        /// <summary>
        /// Presses a key without any modifiers/scancodes.
        /// </summary>
        private void PressVirtualKey(Input.Keyboard.Key key)
        {
            if (key == Input.Keyboard.Key.Unknown)
                return;

            var ev = new KeyEventArgs(
                key,
                false,
                false, false, false, false,
                Scancode.Unknow);

            OnKeyPressed?.Invoke(ev);
        }

        /// <summary>
        /// Releases a key without any modifiers/scancodes.
        /// </summary>
        private void ReleaseVirtualKey(Input.Keyboard.Key key)
        {
            if (key == Input.Keyboard.Key.Unknown)
                return;

            var ev = new KeyEventArgs(
                key,
                false,
                false, false, false, false,
                Scancode.Unknow);

            OnKeyReleased?.Invoke(ev);
        }

        private const int PrimaryJoystickID = 0;

        private readonly Dictionary<Input.Keyboard.Key, float> _axisValues = new();
        private readonly float _axisDeadzone = 0.5f; // TODO make configurable

        public override void JoystickGamepadPressed(Joystick joystick, GamepadButton button)
        {
            if (joystick.GetID() != PrimaryJoystickID)
                return;

            var key = Gamepad.GamepadButtonToKey((Input.Gamepad.Button)button);

            PressVirtualKey(key);
        }

        public override void JoystickGamepadReleased(Joystick joystick, GamepadButton button)
        {
            if (joystick.GetID() != PrimaryJoystickID)
                return;

            var key = Gamepad.GamepadButtonToKey((Input.Gamepad.Button)button);

            ReleaseVirtualKey(key);
        }

        public override void JoystickGamepadAxis(Joystick joystick, GamepadAxis axis, float value)
        {
            if (joystick.GetID() != PrimaryJoystickID)
                return;

            var key = Gamepad.GamepadAxisToKey((Gamepad.Axis)axis, value);

            var prevValue = _axisValues.GetValueOrDefault(key);

            if (MathF.Abs(prevValue) < _axisDeadzone && MathF.Abs(value) >= _axisDeadzone)
            {
                PressVirtualKey(key);
            }
            else if (MathF.Abs(prevValue) >= _axisDeadzone && MathF.Abs(value) < _axisDeadzone)
            {
                ReleaseVirtualKey(key);
            }

            _axisValues[key] = value;
        }

        public override void JoystickAdded(Joystick joystick)
        {
            Logger.DebugS("graphics.löve", $"Added joystick: ID={joystick.GetID()}, Name={joystick.GetName()}");
        }

        public override void JoystickRemoved(Joystick joystick)
        {
            Logger.DebugS("graphics.löve", $"Removed joystick: ID={joystick.GetID()}, Name={joystick.GetName()}");

            if (joystick.GetID() != PrimaryJoystickID)
                return;

            // Release all axis binds.
            foreach (var (key, value) in _axisValues)
            {
                if (value >= _axisDeadzone)
                    ReleaseVirtualKey(key);
            }
            _axisValues.Clear();
        }


        public override bool Quit()
        {
            return OnQuit?.Invoke(new QuitEventArgs()) ?? false;
        }

        #endregion
    }
}

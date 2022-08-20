using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using Color = OpenNefia.Core.Maths.Color;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Core.UserInterface
{
    internal enum ErrorHandlerAction
    {
        Continue,
        PopLayer,
    }

    internal sealed class ErrorHandlerLayer : UiLayerWithResult<ErrorHandlerLayer.Args, ErrorHandlerLayer.Result>
    {
        [Dependency] private readonly IClipboardManager _clipboard = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        internal sealed class Args
        {
            public Args(Exception ex)
            {
                Exception = ex;
            }

            public Exception Exception { get; }
        }

        internal sealed class Result
        {
            public ErrorHandlerAction Action { get; set; } = ErrorHandlerAction.Continue;
        }

        public override int? DefaultZOrder => int.MaxValue;
        public override bool ExceptionTolerance => false;

        private FontSpec _font = default!;
        private Exception _exception = default!;
        private List<string> _lines = new();

        public ErrorHandlerLayer()
        {
            CanControlFocus = true;
            EventFilter = UIEventFilterMode.Stop;
            OnKeyBindDown += HandleKeyBindDown;
        }

        private void RewrapText()
        {
            _lines.Clear();

            void AddSplitLines(string str)
            {
                foreach (var line in str.Split("\n"))
                {
                    var (rest, wrapped) = _font.LoveFont.GetWrapS(UIScale, line, PixelWidth - 60);

                    if (wrapped != null)
                        _lines.AddRange(wrapped);
                }
            }
            _lines.Add("Exception encountered!");
            _lines.Add("");
            AddSplitLines(_exception.GetType().FullName + ": " + _exception.Message);

            _lines.Add("");
            _lines.Add("Strike [Enter] to continue, [Escape] to exit current layer, [Ctrl-C] to copy stacktrace.");
            _lines.Add("");

            if (_exception.StackTrace != null)
            {
                AddSplitLines(_exception.StackTrace);
            }

            _lines.Add("");
            _lines.Add("");
        }

        public override void Initialize(Args args)
        {
            _font = new FontSpec(14, color: Color.White);
            _exception = args.Exception;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);

            RewrapText();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs obj)
        {
            if (obj.Function == EngineKeyFunctions.UISelect)
            {
                Finish(new() { Action = ErrorHandlerAction.Continue });
            }
            else if (obj.Function == EngineKeyFunctions.UICancel)
            {
                Finish(new() { Action = ErrorHandlerAction.PopLayer });
            }
            else if (obj.Function == EngineKeyFunctions.TextCopy)
            {
                CopyStackTrace();
            }
        }

        private void CopyStackTrace()
        {
            if (_exception.StackTrace != null)
            {
                _clipboard.SetText($"{_exception.GetType().FullName}: {_exception.Message}\n{_exception.StackTrace}");
                _lines[_lines.Count - 1] = "Copied.";
            }
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.Black.WithAlphaB(128));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, 0, 0, PixelWidth, PixelHeight);

            var pos = new Vector2(30, 30);
            GraphicsEx.SetFont(_font);

            foreach (var line in _lines)
            {
                GraphicsS.PrintS(UIScale, line, pos.X, pos.Y);
                pos.Y += _font.LoveFont.GetHeightV(UIScale);
            }
        }
    }
}

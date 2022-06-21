using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.WispControl;

namespace OpenNefia.Core.UI.Wisp
{
    /// <summary>
    /// <para>
    /// Top-level floating window manager for Wisp-compatible controls.
    /// </para>
    /// <remarks>
    /// NOTE: Until more support for Wisp is added, this layer is where all
    /// Wisp components will be confined to. It should only be used for
    /// developer-facing features until then.
    /// 
    /// This class will be merged into <see cref="UiLayer"/> at some point.
    /// </remarks>
    /// </summary>
    public abstract class WispLayerWithResult<TArgs, TResult> : UiLayerWithResult<TArgs, TResult>, IWispLayer
        where TResult : class
    {
        [Dependency] private IWispManager _wispManager = default!;
        [Dependency] private IGraphics _graphics = default!;

        private UIBox2? _currentScissor;
        private Stack<UIBox2> _scissorStack = new();

        /// <inheritdoc/>
        public WispRoot WispRoot { get; }

        /// <inheritdoc/>
        public LayoutContainer WindowRoot { get; }

        /// <inheritdoc/>
        public PopupContainer ModalRoot { get; }

        public bool Debug { get; set; }
        public bool DebugClipping { get; set; }

        /// <inheritdoc/>
        public Color GlobalTint { get; private set; }

        public WispLayerWithResult()
        {
            CanControlFocus = true;

            WispRoot = new WispRoot()
            {
                Name = nameof(WispRoot),
                EventFilter = UIEventFilterMode.Ignore,
                HorizontalAlignment = HAlignment.Stretch,
                VerticalAlignment = VAlignment.Stretch,
            };

            WindowRoot = new LayoutContainer
            {
                Name = nameof(WindowRoot),
                EventFilter = UIEventFilterMode.Ignore
            };
            WispRoot.AddChild(WindowRoot);

            ModalRoot = new PopupContainer
            {
                Name = nameof(ModalRoot),
                EventFilter = UIEventFilterMode.Ignore
            };
            WispRoot.AddChild(ModalRoot);

            AddChild(WispRoot);
        }

        /// <inheritdoc/>
        public void PushScissor(UIBox2 scissor, bool ignoreParents = false)
        {
            var oldScissor = _currentScissor;
            var newScissor = scissor;

            if (oldScissor != null)
            {
                if (!ignoreParents)
                {
                    // New scissors should be subsets of the parent.
                    newScissor = oldScissor.Value.Intersection(newScissor) ?? new UIBox2();
                }
                _scissorStack.Push(oldScissor.Value);
            }

            _currentScissor = newScissor;
            Love.Graphics.SetScissor(newScissor);
        }

        /// <inheritdoc/>
        public void PopScissor()
        {
            if (_scissorStack.Count == 0)
            {
                _currentScissor = null;
                Love.Graphics.SetScissor();
            }
            else
            {
                _currentScissor = _scissorStack.Pop();
                Love.Graphics.SetScissor(_currentScissor.Value);
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            WispRoot.InvalidateMeasure();
        }

        public override void OnQuery()
        {
            WispRoot.Measure(_graphics.WindowSize);
            WispRoot.Arrange(UIBox2.FromDimensions(Vector2.Zero, _graphics.WindowSize));

            base.OnQuery();

            // TODO move to UserInterfaceManager
            _wispManager.AddRoot(WispRoot);

            WispRoot.StyleSheetUpdate();
            WispRoot.InvalidateMeasure();
            _wispManager.QueueMeasureUpdate(WispRoot);
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();

            // TODO move to UserInterfaceManager
            _wispManager.RemoveRoot(WispRoot);
        }

        private void UpdateRecursive(WispControl control, float dt)
        {
            if (!control.Visible)
                return;

            control.Update(dt);

            foreach (var child in control.Children)
            {
                if (child is WispControl wispChild)
                    UpdateRecursive(wispChild, dt);
            }
        }

        public override void Update(float dt)
        {
            UpdateRecursive(WispRoot, dt);
        }

        private void DrawRecursive(WispControl control, Color tint)
        {
            if (!control.Visible)
                return;

            // TODO: global offsets are where most of the performance problems
            // are. Should probably adapt a drawing handle architecture combined with
            // Love.Graphics.Push to stop having to add offsets unnecessarily.
            var globalPixelRect = control.GlobalPixelRect;

            if (_currentScissor != null)
            {
                // Manual clip test with scissor region as optimization.
                var controlBox = globalPixelRect;
                var clipMargin = control.RectDrawClipMargin;
                var clipTestBox = new UIBox2i(controlBox.Left - clipMargin, controlBox.Top - clipMargin,
                    controlBox.Right + clipMargin, controlBox.Bottom + clipMargin);

                if (!_currentScissor.Value.Intersects(clipTestBox))
                {
                    return;
                }
            }

            // TODO Love.Graphics.Push!
            // WispControl.Draw() is supposed to use local coordinates
            // (no GlobalPosition/X/Y)

            if (control.RectClipContent && !DebugClipping)
            {
                PushScissor(globalPixelRect);
            }

            tint *= control.ActualTint;
            GlobalTint = tint * control.ActualTintSelf;

            control.Draw();

            if (Debug)
            {
                Love.Graphics.SetColor(Color.Red);
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, control.GlobalRect);
            }

            foreach (var child in control.Children)
            {
                if (child is WispControl wispChild)
                    DrawRecursive(wispChild, tint);
            }

            if (control.RectClipContent && !DebugClipping)
            {
                PopScissor();
            }
        }

        public override void Draw()
        {
            _scissorStack.Clear();
            DrawRecursive(WispRoot, Color.White);

            if (Debug)
            {
                var control = UserInterfaceManager.CurrentlyHovered;

                if (control != null)
                {
                    var font = _wispManager.GetStyleFallback<FontSpec>();
                    var mousePos = UserInterfaceManager.MousePositionScaled;
                    Love.Graphics.SetColor(Color.White);
                    Love.Graphics.SetFont(font.LoveFont);
                    GraphicsS.PrintS(UIScale, $"Control: {control.GetType()}", mousePos.X, mousePos.Y);
                    GraphicsS.PrintS(UIScale, $"Bounds: {control.GlobalPixelRect} {control.PixelSize}", mousePos.X, mousePos.Y + font.LoveFont.GetHeightV(UIScale));

                    var color = Color.Gold;
                    color.A = 0.2f;
                    Love.Graphics.SetColor(color);
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, control.GlobalRect);
                }

                control = UserInterfaceManager.ControlFocused;

                if (control != null)
                {
                    var color = Color.Cyan;
                    color.A = 0.2f;
                    Love.Graphics.SetColor(color);
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, control.GlobalRect);
                }
            }
        }
    }
}

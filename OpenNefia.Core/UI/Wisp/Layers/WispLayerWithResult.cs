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
        public void PushScissor(UIBox2 scissor)
        {
            var oldScissor = _currentScissor;
            var newScissor = scissor;

            if (oldScissor != null)
            {
                // New scissors should be subsets of the parent.
                newScissor = oldScissor.Value.Intersection(newScissor) ?? new UIBox2();
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
            var graphics = IoCManager.Resolve<IGraphics>();
            WispRoot.Measure(graphics.WindowSize);
            WispRoot.Arrange(UIBox2.FromDimensions(Vector2.Zero, graphics.WindowSize));

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
            foreach (var child in control.WispChildren)
            {
                UpdateRecursive(child, dt);
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

            // TODO Love.Graphics.Push!
            // WispControl.Draw() is supposed to use local coordinates
            // (no GlobalPosition/X/Y)

            if (control.RectClipContent && !DebugClipping)
            {
                PushScissor(control.GlobalPixelRect);
            }

            tint *= control.ActualTint;
            GlobalTint = tint * control.ActualTintSelf;

            control.Draw();

            if (Debug)
            {
                var color = Color.Red;
                if (UserInterfaceManager.ControlFocused == control)
                    color = Color.Gold;
                else if (UserInterfaceManager.CurrentlyHovered == control)
                    color = Color.LightBlue;
                Love.Graphics.SetColor(color);
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, control.GlobalRect);
            }

            foreach (var child in control.WispChildren)
            {
                DrawRecursive(child, tint);
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
        }
    }
}

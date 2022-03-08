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
    /// Wisp components will be confined to. 
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

        public bool Debug { get; set; }

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

            AddChild(WispRoot);

            // Arrange early so that WispRoot has the correct size set by the time the
            // constructor finishes.
            var graphics = IoCManager.Resolve<IGraphics>();
            WispRoot.Measure(graphics.WindowSize);
            WispRoot.Arrange(UIBox2.FromDimensions(Vector2.Zero, graphics.WindowSize));
        }

        public override void OnQuery()
        {
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

        private void DrawRecursive(WispControl control)
        {
            if (!control.Visible)
                return;

            if (control.RectClipContent)
            {
                if (_currentScissor != null)
                    _scissorStack.Push(_currentScissor.Value);
                _currentScissor = control.GlobalPixelRect;
                Love.Graphics.SetScissor(_currentScissor.Value);
            }

            control.Draw();

            if (Debug)
            {
                var color = Color.Red;
                if (UserInterfaceManager.ControlFocused == control)
                    color = Color.Gold;
                else if (UserInterfaceManager.CurrentlyHovered == control)
                    color = Color.LightBlue;
                Love.Graphics.SetLineWidth(2);
                Love.Graphics.SetColor(color);
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, control.GlobalRect);
                Love.Graphics.SetLineWidth(1);
            }

            foreach (var child in control.WispChildren)
            {
                DrawRecursive(child);
            }

            if (control.RectClipContent)
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
        }

        public override void Draw()
        {
            DrawRecursive(WispRoot);
        }
    }
}

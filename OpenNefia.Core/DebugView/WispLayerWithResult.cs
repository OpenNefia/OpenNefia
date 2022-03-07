using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.WispControl;

namespace OpenNefia.Core.DebugView
{
    /// <summary>
    /// <para>
    /// Top-level floating window manager for Wisp-compatible controls.
    /// </para>
    /// <remarks>
    /// NOTE: Until more support for Wisp is added, this layer is where all
    /// Wisp components will be confined to. It will be merged into
    /// <see cref="UiLayer"/> at some point.
    /// </remarks>
    /// </summary>
    public abstract class WispLayerWithResult<TArgs, TResult> : UiLayerWithResult<TArgs, TResult>, IWispLayer
        where TResult : class
    {
        [Dependency] private IWispManager _wispManager = default!;

        public WispRoot WispRoot { get; }

        public LayoutContainer WindowRoot { get; }

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
            if (control.RectClipContent)
                Love.Graphics.SetScissor(control.GlobalPixelRect);

            control.Draw();

            /*
            var color = Color.Red;
            if (UserInterfaceManager.ControlFocused == control)
                color = Color.Gold;
            else if (UserInterfaceManager.CurrentlyHovered == control)
                color = Color.LightBlue;
            Love.Graphics.SetLineWidth(2);
            Love.Graphics.SetColor(color);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, control.GlobalRect);
            Love.Graphics.SetLineWidth(1);
            */

            foreach (var child in control.WispChildren)
            {
                DrawRecursive(child);
            }

            if (control.RectClipContent)
                Love.Graphics.SetScissor();
        }

        public override void Draw()
        {
            DrawRecursive(WispRoot);
        }
    }
}

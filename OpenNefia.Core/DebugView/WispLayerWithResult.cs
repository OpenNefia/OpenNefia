using Love;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        where TResult: class
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
        }

        public override void OnQuery()
        {
            base.OnQuery();

            WispRoot.StyleSheetUpdate();
            WispRoot.InvalidateMeasure();
            _wispManager.QueueMeasureUpdate(WispRoot);
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
            control.Draw();

            var color = Color.Red;
            if (UserInterfaceManager.ControlFocused == control)
                color = Color.Gold;
            else if (UserInterfaceManager.CurrentlyHovered == control)
                color = Color.LightBlue;
            Love.Graphics.SetLineWidth(2);
            Love.Graphics.SetColor(color);
            GraphicsS.RectangleS(UIScale, DrawMode.Line, control.GlobalRect);
            Love.Graphics.SetLineWidth(1);

            foreach (var child in control.WispChildren)
            {
                DrawRecursive(child);
            }
        }

        public override void Draw()
        {
            DrawRecursive(WispRoot);
        }
    }
}

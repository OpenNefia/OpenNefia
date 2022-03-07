using Love;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Wisp;
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
    /// Top-level floating window manager for debug functions.
    /// </para>
    /// <para>
    /// NOTE: Until more support for Wisp is added, this layer is where all
    /// Wisp components will be confined to.
    /// </para>
    /// </summary>
    public sealed class DebugViewLayer : UiLayerWithResult<UINone, UINone>
    {
        [Dependency] private IWispManager _wispManager = default!;

        private WispRoot WispRoot;

        public DebugViewLayer()
        {
            WispRoot = new WispRoot()
            {
                Name = nameof(WispRoot),
                EventFilter = UIEventFilterMode.Ignore,
                HorizontalAlignment = HAlignment.Stretch,
                VerticalAlignment = VAlignment.Stretch,
            };

            WispRoot.AddChild(new DefaultWindow()
            {
                TitleClass = "windowTitleAlert",
                HeaderClass = "windowHeaderAlert",
                Title = "Asdfg!",
                PreferredSize = (400, 200),
            });

            AddChild(WispRoot);
        }

        public override void Initialize(UINone args)
        {
            // WispRoot.StyleSheetUpdate();
            WispRoot.InvalidateMeasure();
            _wispManager.QueueMeasureUpdate(WispRoot);
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
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

            Love.Graphics.SetColor(Color.Red);
            GraphicsS.RectangleS(UIScale, DrawMode.Line, control.Rect);
            
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

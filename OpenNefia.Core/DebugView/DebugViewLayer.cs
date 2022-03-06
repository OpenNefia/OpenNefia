using Love;
using OpenNefia.Core.Locale;
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
        }

        private void UpdateRecursive(UiElement control, float dt)
        {
            control.Update(dt);
            foreach (var child in control.Children)
            {
                UpdateRecursive(child, dt);
            }
        }

        public override void Update(float dt)
        {
            UpdateRecursive(WispRoot, dt);
        }

        private void DrawRecursive(UiElement control)
        {
            control.Draw();
            foreach (var child in control.Children)
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

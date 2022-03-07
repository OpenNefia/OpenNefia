using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;

namespace OpenNefia.Core.DebugView
{
    public sealed class DebugViewLayer : WispLayerWithResult<UINone, UINone>
    {
        public DebugViewLayer()
        {
            var win = new DefaultWindow()
            {
                TitleClass = DefaultWindow.StyleClassWindowTitleAlert,
                HeaderClass = DefaultWindow.StyleClassWindowHeaderAlert,
                Title = "Asdfg!",
                PreferredSize = (400, 100),
                EventFilter = UserInterface.UIEventFilterMode.Pass,
            };

            win.SetValue(LayoutContainer.DebugProperty, true);

            WindowRoot.AddChild(win);
            WindowRoot.Debug = true;
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }
    }
}

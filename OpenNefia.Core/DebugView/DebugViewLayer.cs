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
            for (int i = 0; i < 5; i++)
            {
                var win = new DefaultWindow()
                {
                    TitleClass = DefaultWindow.StyleClassWindowTitleAlert,
                    HeaderClass = DefaultWindow.StyleClassWindowHeaderAlert,
                    Title = "Asdfg!",
                    PreferredSize = (400, 100),
                    EventFilter = UserInterface.UIEventFilterMode.Pass,
                };

                // win.SetValue(LayoutContainer.DebugProperty, true);

                var box = new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical
                };

                for (int j = 0; j < 3; j++)
                {
                    box.AddChild(new Label()
                    {
                        Text = $"テスト{j}でござる。",
                        HorizontalAlignment = UI.Wisp.WispControl.HAlignment.Left,
                        HorizontalExpand = true
                    });
                }

                win.Contents.AddChild(box);
                WindowRoot.AddChild(win);

                win.OpenCentered();
            }

            // WindowRoot.Debug = true;
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

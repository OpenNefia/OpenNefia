using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.WispControl;
using OpenNefia.Core.ControlTest;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.ViewVariables;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.DebugView
{
    public interface IDebugViewLayer : IUiLayerWithResult<UINone, UINone>, IWispLayer
    {
    }

    public sealed class DebugViewLayer : WispLayerWithResult<UINone, UINone>, IDebugViewLayer
    {
        private bool _initialized = false;

        public DebugViewLayer()
        {
            OnKeyBindDown += KeyBindDown;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            for (int i = 0; i < 5; i++)
            {
                var win = new DefaultWindow()
                {
                    TitleClass = DefaultWindow.StyleClassWindowTitleAlert,
                    HeaderClass = DefaultWindow.StyleClassWindowHeaderAlert,
                    Title = "Asdfg!",
                    ExactSize = (400, 100),
                    EventFilter = UIEventFilterMode.Pass,
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
                        HorizontalAlignment = HAlignment.Left,
                        HorizontalExpand = true
                    });
                }

                win.Contents.AddChild(box);
                this.OpenWindowCentered(win);
            }

            var controlTestWindow = new ControlTestMainWindow();
            this.OpenWindowToLeft(controlTestWindow);

            var controlDebugWindow = new ControlDebugWindow();
            this.OpenWindowCentered(controlDebugWindow);

            _initialized = true;
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Initialize();
        }

        private void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }
    }
}

using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlDebugWindow : DefaultWindow
    {
        public ControlDebugWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            ToggleDebugButton.OnPressed += ToggleDebug;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs obj)
        {
            WispRootLayer!.Debug = !WispRootLayer.Debug;
        }
    }
}

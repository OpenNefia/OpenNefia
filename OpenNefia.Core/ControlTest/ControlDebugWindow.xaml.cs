using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.UserInterface.XAML.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.ControlDesigner;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlDebugWindow : DefaultWindow
    {
        public ControlDebugWindow()
        {
            IoCManager.InjectDependencies(this);
            OpenNefiaXamlLoader.Load(this);

            ToggleDebugButton.OnPressed += ToggleDebug;
            DesignerButton.OnPressed += OpenDesigner;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs obj)
        {
            WispRootLayer!.Debug = !WispRootLayer.Debug;
        }

        private void OpenDesigner(BaseButton.ButtonEventArgs obj)
        {
            UserInterfaceManager.Query<ControlDesignerLayer>();
        }
    }
}

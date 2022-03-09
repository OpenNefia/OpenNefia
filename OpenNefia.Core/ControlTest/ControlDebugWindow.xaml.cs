using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.UserInterface.XAML.HotReload;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlDebugWindow : DefaultWindow
    {
        [Dependency] private readonly IXamlHotReloadManager _xamlHotReload = default!;

        public ControlDebugWindow()
        {
            IoCManager.InjectDependencies(this);
            OpenNefiaXamlLoader.Load(this);

            ToggleDebugButton.OnPressed += ToggleDebug;
            PatchButton.OnPressed += DoPatch;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs obj)
        {
            WispRootLayer!.Debug = !WispRootLayer.Debug;
        }

        private void DoPatch(BaseButton.ButtonEventArgs obj)
        {
            _xamlHotReload.HotReloadXamlControl(typeof(TextureRectWindow), "C:\\Users\\yuno\\build\\OpenNefia.NET\\OpenNefia.Core\\ControlTest\\TextureRectWindow.xaml");
        }
    }
}

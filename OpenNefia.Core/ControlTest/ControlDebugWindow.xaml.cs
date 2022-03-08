using HarmonyLib;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenNefia.Core.ControlTest
{
    public partial class ControlDebugWindow : DefaultWindow
    {
        public ControlDebugWindow()
        {
            OpenNefiaXamlLoader.Load(this);

            ToggleDebugButton.OnPressed += ToggleDebug;
            PatchButton.OnPressed += DoPatch;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs obj)
        {
            WispRootLayer!.Debug = !WispRootLayer.Debug;
        }

        private bool _patched = false;

        private void DoPatch(BaseButton.ButtonEventArgs obj)
        {
            if (_patched)
                return;

            var harmony = new Harmony("xyz.opennefia");
            var original = typeof(TextureRectWindow).GetMethod("!XamlIlPopulateTrampoline", BindingFlags.NonPublic | BindingFlags.Static);
            var transpiler = typeof(Patch).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);
            harmony.Patch(original, transpiler: new HarmonyMethod(transpiler));
            Logger.Info("Patched trampoline.");

            _patched = true;
        }
    }


    public static class Patch
    {
        static void Method(IServiceProvider provider, DefaultWindow win)
        {
            Logger.Info("Patch hit!");
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new List<CodeInstruction>()
            {
                new(OpCodes.Ldnull),
                new(OpCodes.Ldarg_0),
                CodeInstruction.Call(typeof(Patch), nameof(Method)),
                new(OpCodes.Ret)
            };
        }
    }
}

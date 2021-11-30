using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Hud;

namespace OpenNefia.Core.Logic
{
    public static class Mes
    {
        public static void Display(string text, Color? color = null)
        {
            IoCManager.Resolve<IHudLayer>().MessageWindow.Print(text, color);
        }
    }
}

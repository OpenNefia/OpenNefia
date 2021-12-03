using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Logic
{
    public static class Mes
    {
        public static void Display(string text, Color? color = null)
        {
            IoCManager.Resolve<IHudLayer>().MessageWindow.Print(text, color);
        }
    }
}

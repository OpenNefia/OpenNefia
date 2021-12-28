using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Logic
{
    [Obsolete]
    public static class Mes
    {
        public static void Display(string text, Color? color = null, bool noCapitalize = false)
        {
            if (!noCapitalize)
                text = Loc.Capitalize(text);
            
            IoCManager.Resolve<IHudLayer>().MessageWindow.Print(text, color);
        }

        public static void Newline()
        {
            // TODO
        }
    }
}

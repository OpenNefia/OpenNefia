using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Logic
{
    [Obsolete("Convert to IoC dependency")]
    public static class Mes
    {
        public static void Display(string text, Color? color = null, bool newLine = true, bool noCapitalize = false)
        {
            if (!noCapitalize)
                text = Loc.Capitalize(text);
            
            IoCManager.Resolve<IHudLayer>().MessageWindow.Print(text, color, newLine);
        }

        public static void Newline()
        {
            // TODO
        }

        [Obsolete("Maybe move `entity` into a named param for Display()?")]
        public static void DisplayIfLos(EntityUid entity, string mes, Color? color = null, bool noCapitalize = false)
        {
            var visibility = EntitySystem.Get<IVisibilitySystem>();
            var gameSession = IoCManager.Resolve<IGameSessionManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            if (entMan.IsAlive(gameSession.Player) 
                && visibility.HasLineOfSight(gameSession.Player, entity))
            {
                Mes.Display(mes, color, noCapitalize);
            }
        }
    }
}

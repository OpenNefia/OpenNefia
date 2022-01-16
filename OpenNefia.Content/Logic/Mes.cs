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
    public interface IMessage
    {
        void Newline();
        void Display(string text, Color? color = null, bool noCapitalize = false);
        [Obsolete("Maybe move `entity` into a named param for Display()?")]
        void DisplayIfLos(EntityUid entity, string mes, Color? color = null, bool noCapitalize = false);
    }

    public class Message : IMessage
    {
        [Dependency] private readonly IHudLayer _hud = default!;

        public void Newline()
        {
            _hud.MessageWindow.Newline();
        }

        public void Display(string text, Color? color = null, bool noCapitalize = false)
        {
            if (!noCapitalize)
                text = Loc.Capitalize(text);
            
            _hud.MessageWindow.Print(text, color);
        }

        public void DisplayIfLos(EntityUid entity, string mes, Color? color = null, bool noCapitalize = false)
        {
            var visibility = EntitySystem.Get<IVisibilitySystem>();
            var gameSession = IoCManager.Resolve<IGameSessionManager>();
            var entMan = IoCManager.Resolve<IEntityManager>();
            if (entMan.IsAlive(gameSession.Player) 
                && visibility.HasLineOfSight(gameSession.Player, entity))
            {
                Display(mes, color, noCapitalize);
            }
        }
    }
}

using OpenNefia.Content.GameObjects;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Logic
{
    public interface IMessagesManager
    {
        void Newline();
        void Display(string text, Color? color = null, bool alert = false, bool noCapitalize = false, EntityUid? entity = null);
    }

    public class MessagesManager : IMessagesManager
    {
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public void Newline()
        {
            _hud.MessageWindow?.Newline();
        }

        public void Display(string text, Color? color = null, bool alert = false, bool noCapitalize = false, EntityUid? entity = null)
        {
            if (entity != null)
            {
                var visibilitySys = EntitySystem.Get<IVisibilitySystem>();
                var canSee = _entityManager.IsAlive(_gameSession.Player) 
                    && visibilitySys.HasLineOfSight(_gameSession.Player, entity.Value);

                if (!canSee)
                    return;
            }

            if (!noCapitalize)
                text = Loc.Capitalize(text);
            
            _hud.MessageWindow?.Print(text, color);

            // >>>>>>>> elona122/shade2/init.hsp:3570 	if msgAlert@=true:if cfg_alert@>1{ ...
            if (alert)
            {
                var wait = _config.GetCVar(CCVars.AnimeAlertWait);

                while (wait > 0f)
                {
                    wait -= _gameController.StepFrame();
                }
            }
            // <<<<<<<< elona122/shade2/init.hsp:3581 		} ...
        }
    }

    public static class IMessagesManagerExt
    {
        public static void DisplayL(this IMessagesManager mes, LocaleKey key, Color? color = null, bool alert = false, bool noCapitalize = false, EntityUid? entity = null)
            => mes.Display(Loc.GetString(key), color, alert, noCapitalize, entity);
    }
}

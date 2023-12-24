using OpenNefia.Content.GameController;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.Visibility;
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
        void Display(string text, Color? color = null, bool alert = false, bool noCapitalize = false, bool combineDuplicates = false, EntityUid? entity = null);
        void Alert();
        void Clear();
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

        public void Display(string text, Color? color = null, bool alert = false, bool noCapitalize = false, bool combineDuplicates = false, EntityUid? entity = null)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (entity != null)
            {
                var visibilitySys = EntitySystem.Get<IVisibilitySystem>();
                var canSee = _entityManager.IsAlive(_gameSession.Player)
                    && visibilitySys.PlayerCanSeeEntity(entity.Value);

                if (!canSee)
                    return;
            }

            if (!noCapitalize)
                text = Loc.Capitalize(text);

            _hud.MessageWindow?.Print(text, color);

            // >>>>>>>> elona122/shade2/init.hsp:3570 	if msgAlert@=true:if cfg_alert@>1{ ...
            if (alert)
                Alert();
            // <<<<<<<< elona122/shade2/init.hsp:3581 		} ...
        }

        public void Alert()
        {
            var wait = _config.GetCVar(CCVars.AnimeAlertWait);
            
            // TODO
            // while (wait > 0f)
            //     wait -= _gameController.StepFrame();
        }

        public void Clear()
        {
            _hud.MessageWindow?.Clear();
        }
    }
}

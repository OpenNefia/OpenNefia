using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Log;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Wishes
{
    public interface IWishSystem : IEntitySystem
    {
        /// <summary>
        /// Queries the player for a wish and grants it.
        /// </summary>
        /// <returns>True if the wish succeeded.</returns>
        bool PromptForWish(EntityUid? wisher = null);

        /// <summary>
        /// Grants a wish.
        /// </summary>
        /// <param name="wisher"></param>
        /// <param name="wish"></param>
        /// <returns>True if the wish succeeded.</returns>
        bool GrantWish(string wish, EntityUid? wisher = null, bool silent = false);
    }

    public sealed class WishSystem : EntitySystem, IWishSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMan = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
        }

        public bool PromptForWish(EntityUid? wisher = null)
        {
            wisher ??= _gameSession.Player;

            var args = new TextPrompt.Args()
            {
                QueryText = Loc.GetString("Elona.Wishes.Prompt", ("wisher", wisher.Value)),
                QueryTextColor = UiColors.MesYellow,
                MaxLength = 32,
                IsCancellable = false
            };

            var result = _uiMan.Query<TextPrompt, TextPrompt.Args, TextPrompt.Result>(args);

            if (!result.HasValue || string.IsNullOrWhiteSpace(result.Value.Text))
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                return false;
            }

            var wish = result.Value.Text;
            return GrantWish(wish, wisher.Value);
        }

        public bool GrantWish(string wish, EntityUid? wisher = null, bool silent = false)
        {
            wisher ??= _gameSession.Player;

            if (!silent)
                _audio.Play(Protos.Sound.Ding2);

            wish = wish.Trim();

            _mes.Display(Loc.GetString("Elona.Wishes.YouWish", ("wisher", wisher.Value), ("wish", wish)), color: UiColors.MesTalk);

            var didSomething = false;

            foreach (var handler in _protos.EnumeratePrototypes<WishHandlerPrototype>())
            {
                if (Loc.PrototypeKeyExists(handler, "Keyword"))
                {
                    var candidates = new List<string>();
                    if (Loc.TryGetPrototypeList(handler, "Keyword", out var list))
                    {
                        candidates.AddRange(list);
                    }
                    else if (Loc.TryGetPrototypeString(handler, "Keyword", out var str))
                    {
                        candidates.Add(str);
                    }
                    else
                    {
                        Logger.ErrorS("wish", $"Could not find valid match text for wish handler {handler.ID}");
                    }

                    var matched = false;

                    foreach (var cand in candidates)
                    {
                        if (wish.Contains(cand, StringComparison.InvariantCultureIgnoreCase))
                        {
                            matched = true;
                            break;
                        }
                    }

                    if (!matched)
                        continue;
                }

                var ev = new P_WishHandlerOnWishEvent(wisher.Value, wish);
                _protos.EventBus.RaiseEvent(handler, ref ev);
                if (ev.Handled)
                {
                    didSomething = ev.OutDidSomething;
                    break;
                }
            }

            if (!didSomething)
            {
                _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                return false;
            }

            // TODO net

            return true;
        }
    }
}
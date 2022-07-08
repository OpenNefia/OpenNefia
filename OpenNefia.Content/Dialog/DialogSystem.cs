using NativeFileDialogSharp;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public interface IDialogSystem : IEntitySystem
    {
        void ModifyImpression(EntityUid uid, int delta, DialogComponent? dialog = null);
    }

    public sealed class DialogSystem : EntitySystem, IDialogSystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;

        public override void Initialize()
        {
            SubscribeComponent<DialogComponent, EntityBeingGeneratedEvent>(InitializePersonality);
            SubscribeEntity<CheckKillEvent>(ProcImpressionChange);
        }

        private void InitializePersonality(EntityUid uid, DialogComponent component, ref EntityBeingGeneratedEvent args)
        {
            // TODO: replace with custom talk
            component.Personality = _rand.Next(4);
        }

        public void ModifyImpression(EntityUid uid, int delta, DialogComponent? dialog = null)
        {
            if (!Resolve(uid, ref dialog))
                return;

            // TODO
        }

        private void ProcImpressionChange(EntityUid victim, ref CheckKillEvent args)
        {
            if (!(_parties.IsInPlayerParty(args.Attacker) && !_parties.IsInPlayerParty(victim)))
                return;

            if (HasComp<RoleAdventurerComponent>(victim))
                ModifyImpression(victim, -25);

            if (!_gameSession.IsPlayer(args.Attacker) && TryComp<DialogComponent>(args.Attacker, out var dialog))
            {
                if (dialog.Impression < Impressions.Marry)
                {
                    if (_rand.OneIn(2))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, "Elona.Heart", 3);
                    }
                }
                else
                {
                    if (_rand.OneIn(10))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, "Elona.Heart", 3);
                    }
                }
            }
        }
    }
}
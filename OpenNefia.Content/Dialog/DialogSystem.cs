using NativeFileDialogSharp;
using OpenNefia.Content.Damage;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Roles;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Console;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
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
        int GetImpressionLevel(int impression);
        TurnResult TryToChatWith(EntityUid source, EntityUid target, bool force = false, PrototypeId<DialogPrototype>? dialogID = null);
        TurnResult StartDialog(EntityUid source, EntityUid target, PrototypeId<DialogPrototype> dialogID);
        string GetDefaultSpeakerName(EntityUid uid);
    }

    public sealed partial class DialogSystem : EntitySystem, IDialogSystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ICSharpReplExecutor _compiler = default!;

        public override void Initialize()
        {
            SubscribeComponent<DialogComponent, EntityBeingGeneratedEvent>(InitializePersonality);
            SubscribeEntity<CheckKillEvent>(ProcImpressionChange);
            SubscribeComponent<DialogComponent, WasCollidedWithEventArgs>(HandledCollidedWith, priority: EventPriorities.Low + 1000);

            _protos.PrototypesReloaded += CompileDialogs;
        }

        private void InitializePersonality(EntityUid uid, DialogComponent component, ref EntityBeingGeneratedEvent args)
        {
            // TODO: replace with custom talk
            component.Personality = _rand.Next(4);
        }

        private void ProcImpressionChange(EntityUid victim, ref CheckKillEvent args)
        {
            if (!(_parties.IsInPlayerParty(args.Attacker) && !_parties.IsInPlayerParty(victim)))
                return;

            if (HasComp<RoleAdventurerComponent>(victim))
                ModifyImpression(victim, -25);

            if (!_gameSession.IsPlayer(args.Attacker) && TryComp<DialogComponent>(args.Attacker, out var dialog))
            {
                if (dialog.Impression < ImpressionLevels.Marry)
                {
                    if (_rand.OneIn(2))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, EmotionIcons.Heart, 3);
                    }
                }
                else
                {
                    if (_rand.OneIn(10))
                    {
                        ModifyImpression(args.Attacker, 1);
                        _emoIcons.SetEmotionIcon(args.Attacker, EmotionIcons.Heart, 3);
                    }
                }
            }
        }

        private void HandledCollidedWith(EntityUid uid, DialogComponent component, WasCollidedWithEventArgs args)
        {
            if (args.Handled)
                return;

            if (!_gameSession.IsPlayer(args.Source))
                return;

            args.Handle(TryToChatWith(args.Source, uid));
        }

        public int GetImpressionLevel(int impression)
        {
            if (impression < ImpressionLevels.Foe)
                return 0;
            else if (impression < ImpressionLevels.Hate)
                return 1;
            else if (impression < ImpressionLevels.Normal - 10)
                return 2;
            else if (impression < ImpressionLevels.Amiable)
                return 3;
            else if (impression < ImpressionLevels.Friend)
                return 4;
            else if (impression < ImpressionLevels.Fellow)
                return 5;
            else if (impression < ImpressionLevels.Marry)
                return 6;
            else if (impression < ImpressionLevels.Soulmate)
                return 7;
            else
                return 8;
        }

        public void ModifyImpression(EntityUid uid, int delta, DialogComponent? dialog = null)
        {
            if (!Resolve(uid, ref dialog))
                return;

            var level = GetImpressionLevel(dialog.Impression);
            if (delta >= 0)
            {
                delta = delta * 100 / (50 + level * level * level);
                if (delta == 0 && level < _rand.Next(10))
                    delta = 1;
            }

            dialog.Impression += delta;

            var newLevel = GetImpressionLevel(dialog.Impression);
            var newLevelText = Loc.GetString($"Elona.Dialog.Impression.Levels.{newLevel}");
            if (level > newLevel)
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Impression.Modify.Lose", ("chara", uid), ("newLevel", newLevelText)), UiColors.MesPurple);
            }
            else if (newLevel > level && _factions.GetRelationTowards(uid, _gameSession.Player) > Relation.Enemy)
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Impression.Modify.Gain", ("chara", uid), ("newLevel", newLevelText)), UiColors.MesGreen);
            }
        }
    }
}
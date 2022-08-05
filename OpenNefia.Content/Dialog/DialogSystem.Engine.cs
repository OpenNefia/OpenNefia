using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.World;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class DialogSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IReligionSystem _religion = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public TurnResult TryToChatWith(EntityUid source, EntityUid target, bool force = false, PrototypeId<DialogPrototype>? dialogID = null)
        {
            if (_factions.IsPlayer(target))
                return TurnResult.Failed;

            if (!TryComp<DialogComponent>(target, out var dialog) 
                || !dialog.CanTalk
                || (_factions.GetRelationTowards(target, source) <= Factions.Relation.Dislike && !force))
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Common.WillNotListen", ("entity", target)));
                return TurnResult.Failed;
            }

            dialogID ??= dialog.DialogID ?? Protos.Dialog.Default;

            if (_world.State.GameDate >= dialog.InterestRenewDate)
                dialog.Interest = 100;

            if (_effects.HasEffect(target, Protos.StatusEffect.Sleep))
                return StartDialog(source, target, Protos.Dialog.IsSleeping);
        
            if (_activities.HasAnyActivity(target))
                return StartDialog(source, target, Protos.Dialog.IsBusy);

            return StartDialog(source, target, dialogID.Value);
        }

        public TurnResult StartDialog(EntityUid source, EntityUid target, PrototypeId<DialogPrototype> dialogID)
        {
            if (!IsAlive(target))
                return TurnResult.Aborted;

            var args = new DialogArgs()
            {
            };
            IDialogLayer dialogLayer = _uiManager.CreateLayer<DialogLayer, DialogArgs, DialogResult>(args);

            var dialogProto = _protos.Index(dialogID);
            var engine = new DialogEngine(source, target, dialogProto, dialogLayer);

            return engine.StartDialog();
        }

        public string GetDefaultSpeakerName(EntityUid uid)
        {
            var name = _displayNames.GetDisplayName(uid);

            var gender = EntityManager.GetComponentOrNull<CharaComponent>(uid)?.Gender ?? Gender.Unknown;
            name += " " + Loc.GetString($"Elona.Gender.Names.{gender}.Polite");

            if (EntityManager.TryGetComponent<FameComponent>(uid, out var fame))
                name += " " + Loc.GetString("Elona.Dialog.SpeakerName.Fame", ("fame", fame.Fame.Buffed));

            if (EntityManager.TryGetComponent<RoleShopkeeperComponent>(uid, out var shopkeeper))
                name += " " + Loc.GetString("Elona.Dialog.SpeakerName.ShopRank", ("shopRank", shopkeeper.ShopRank));

            if (EntityManager.GetComponentOrNull<CommonProtectionsComponent>(_gameSession.Player)?.CanCatchGodSignals ?? false)
                name += $" ({_religion.GetGodName(uid)})";

            if (_config.GetCVar(CCVars.DebugShowImpression) && EntityManager.TryGetComponent<DialogComponent>(uid, out var dialog))
                name += $" imp:{dialog.Impression}";

            return name;
        }
    }
}

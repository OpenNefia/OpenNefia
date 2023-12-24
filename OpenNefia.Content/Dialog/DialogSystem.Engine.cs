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
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Console;
using OpenNefia.Core.Log;

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

        /// <inheritdoc/>
        public TurnResult TryToChatWith(EntityUid source, EntityUid target, PrototypeId<DialogPrototype> dialogID, Blackboard<IDialogExtraData>? extraData = null, bool force = false)
        {
            var proto = _protos.Index(dialogID);
            return TryToChatWith(source, target, new QualifiedDialogNodeID(dialogID, proto.StartNode), extraData, force);
        }

        /// <inheritdoc/>
        public TurnResult TryToChatWith(EntityUid source, EntityUid target, QualifiedDialogNodeID? dialogNodeID = null, Blackboard<IDialogExtraData>? extraData = null, bool force = false)
        {
            if (_factions.IsPlayer(target))
                return TurnResult.Failed;

            if (!TryComp<DialogComponent>(target, out var dialog) 
                || (_factions.GetRelationTowards(target, source) <= Factions.Relation.Dislike && !force))
            {
                _mes.Display(Loc.GetString("Elona.Dialog.Common.WillNotListen", ("entity", target)));
                return TurnResult.Failed;
            }

            if (dialogNodeID == null)
            {
                PrototypeId<DialogPrototype> dialogID;
                if (dialog.DialogID != null)
                {
                    dialogID = dialog.DialogID.Value;
                }
                else
                {
                    dialogID = Protos.Dialog.Default;
                }
                dialogNodeID = new QualifiedDialogNodeID(dialogID, _protos.Index(dialogID).StartNode);
            }

            if (_world.State.GameDate >= dialog.InterestRenewDate)
                dialog.Interest = 100;

            if (_effects.HasEffect(target, Protos.StatusEffect.Sleep))
                return StartDialog(source, target, Protos.Dialog.IsSleeping);
        
            if (_activities.HasAnyActivity(target))
                return StartDialog(source, target, Protos.Dialog.IsBusy);

            return StartDialog(source, target, dialogNodeID.Value, extraData);
        }

        /// <inheritdoc/>
        public TurnResult StartDialog(EntityUid source, EntityUid target, PrototypeId<DialogPrototype> dialogID, Blackboard<IDialogExtraData>? extraData = null)
        {
            var proto = _protos.Index(dialogID);
            return StartDialog(source, target, new QualifiedDialogNodeID(dialogID, proto.StartNode), extraData);
        }

        /// <inheritdoc/>
        public TurnResult StartDialog(EntityUid source, EntityUid target, QualifiedDialogNodeID dialogNodeID, Blackboard<IDialogExtraData>? extraData = null)
        {
            if (!IsAlive(target))
                return TurnResult.Aborted;

            var args = new DialogArgs()
            {
            };
            IDialogLayer dialogLayer = _uiManager.CreateAndInitializeLayer<DialogLayer, DialogArgs, DialogResult>(args);

            var engine = new DialogEngine(source, target, dialogLayer, extraData);

            return engine.StartDialog(dialogNodeID);
        }

        public string GetDefaultSpeakerName(EntityUid uid)
        {
            var name = _displayNames.GetDisplayName(uid);

            var gender = CompOrNull<CharaComponent>(uid)?.Gender ?? Gender.Unknown;
            name += " " + Loc.GetString($"Elona.Gender.Names.{gender}.Polite");

            if (TryComp<FameComponent>(uid, out var fame))
                name += " " + Loc.GetString("Elona.Dialog.SpeakerName.Fame", ("fame", fame.Fame.Buffed));

            if (TryComp<RoleShopkeeperComponent>(uid, out var shopkeeper))
                name += " " + Loc.GetString("Elona.Dialog.SpeakerName.ShopRank", ("shopRank", shopkeeper.ShopRank));

            if (CompOrNull<CommonProtectionsComponent>(_gameSession.Player)?.CanDetectReligion.Buffed ?? false)
                name += $" ({_religion.GetGodName(uid)})";

            if (_config.GetCVar(CCVars.DebugShowImpression) && TryComp<DialogComponent>(uid, out var dialog))
                name += $" imp:{dialog.Impression}";

            return name;
        }
    }
}

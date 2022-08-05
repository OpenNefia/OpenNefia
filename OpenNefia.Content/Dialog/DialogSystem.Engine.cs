using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Fame;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Religion;
using OpenNefia.Content.Shopkeeper;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public sealed partial class DialogSystem
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IReligionSystem _religion = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public TurnResult StartDialog(EntityUid target, PrototypeId<DialogPrototype> dialogID)
        {
            if (!IsAlive(target))
                return TurnResult.Aborted;

            var args = new DialogArgs()
            {
            };
            IDialogLayer dialogLayer = _uiManager.CreateLayer<DialogLayer, DialogArgs, DialogResult>(args);

            var dialogProto = _protos.Index(dialogID);
            var engine = new DialogEngine(_gameSession.Player, target, dialogProto, dialogLayer);

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

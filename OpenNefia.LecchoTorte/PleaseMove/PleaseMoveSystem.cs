using OpenNefia.Content.Dialog;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Areas;
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
using OpenNefia.Content.Factions;
using OpenNefia.Content.ShowHouse;
using OpenNefia.Content.Parties;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Core.Audio;
using Love;
using OpenNefia.Content.World;
using NetVips;
using OpenNefia.Content.UI;

namespace OpenNefia.LecchoTorte.PleaseMove
{
    /// <summary>
    /// Adds a "move aside" dialog option to the default dialog. Ported from Elona+.
    /// </summary>
    public sealed class PleaseMoveSystem : EntitySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IMoveableSystem _moveable = default!;

        public override void Initialize()
        {
            SubscribeEntity<GetDefaultDialogChoicesEvent>(AddPleaseMoveChoice);
        }

        private void AddPleaseMoveChoice(EntityUid uid, GetDefaultDialogChoicesEvent args)
        {
            if (CompOrNull<QualityComponent>(uid)?.Quality.Base != Quality.Unique
                && _factions.GetRelationToPlayer(uid) == Relation.Neutral
                && TryMap(uid, out var map)
                && !HasComp<MapShowHouseComponent>(map.MapEntityUid)
                && !_parties.IsInPlayerParty(uid)
                && !_deferredEvents.IsEventEnqueued())
            {
                args.OutChoices.Add(new()
                {
                    Text = DialogTextEntry.FromLocaleKey("LecchoTorte.PleaseMove.Dialog.Choice"),
                    NextNode = new(new("LecchoTorte.PleaseMove"), "Execute")
                });
            }
        }

        public QualifiedDialogNode? PleaseMove_Execute(IDialogEngine engine, IDialogNode node)
        {
            if (!TryComp<DialogComponent>(engine.Speaker, out var dialog))
                return null;

            if (dialog.Interest <= 0)
            {
                _audio.Play(Protos.Sound.Fail1);
                _mes.Display(Loc.GetString("LecchoTorte.PleaseMove.Refuses", ("speaker", engine.Speaker), ("player", engine.Player)), entity: engine.Speaker);
                return null;
            }

            dialog.Interest -= 20;
            dialog.InterestRenewDate = _world.State.GameDate + GameTimeSpan.FromHours(12);

            _mes.Display(Loc.GetString("LecchoTorte.PleaseMove.Response", ("speaker", engine.Speaker), ("player", engine.Player)), color: UiColors.MesTalk, entity: engine.Speaker.Value);
            _mes.Display(Loc.GetString("Elona.Movement.Displace.Text", ("source", engine.Player), ("target", engine.Speaker)));
            _moveable.SwapPlaces(engine.Player, engine.Speaker.Value);

            return null;
        }
    }
}
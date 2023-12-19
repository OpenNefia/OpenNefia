using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Content.Identify;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Book;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly IAncientBookSystem _ancientBook = default!;

        private void Initialize_ReadingAncientBook()
        {
            SubscribeComponent<ActivityReadingAncientBookComponent, OnActivityStartEvent>(ReadingAncientBook_OnStart);
            SubscribeComponent<ActivityReadingAncientBookComponent, OnActivityPassTurnEvent>(ReadingAncientBook_OnPassTurn);
            SubscribeComponent<ActivityReadingAncientBookComponent, OnActivityFinishEvent>(ReadingAncientBook_OnFinish);
            SubscribeComponent<ActivityReadingAncientBookComponent, OnActivityCleanupEvent>(ReadingAncientBook_OnCleanup);
        }

        private void ReadingAncientBook_OnStart(EntityUid activity, ActivityReadingAncientBookComponent component, OnActivityStartEvent args)
        {
            if (!IsAlive(component.AncientBook))
            {
                args.Cancel();
                return;
            }

            if (TryComp<AncientBookComponent>(component.AncientBook, out var ancientBook)
                && ancientBook.IsDecoded)
            {
                _mes.Display(Loc.GetString("Elona.Read.AncientBook.AlreadyDecoded", ("entity", args.Activity.Actor), ("item", component.AncientBook)));
            }

            _mes.Display(Loc.GetString("Elona.Read.Activity.Start", ("entity", args.Activity.Actor), ("item", component.AncientBook)));
            _inUse.SetItemInUse(args.Activity.Actor, component.AncientBook);
        }

        private void ReadingAncientBook_OnPassTurn(EntityUid activity, ActivityReadingAncientBookComponent component, OnActivityPassTurnEvent args)
        {
            var actor = args.Activity.Actor;

            _skills.GainSkillExp(actor, Protos.Skill.Literacy, 15, 10, 100);

            if (TryComp<AncientBookComponent>(component.AncientBook, out var ancientBook))
            {
                int difficulty = _ancientBook.GetDecodeDifficulty(component.AncientBook, ancientBook);

                if (_curseStates.IsBlessed(component.AncientBook))
                    difficulty = (int)(difficulty / 1.2);
                else if (_curseStates.IsCursed(component.AncientBook))
                    difficulty = (int)(difficulty * 1.5);

                var magicStat = _skills.Level(actor, Protos.Skill.AttrMagic);
                var success = _spellbooks.TryToReadSpellbook(actor, difficulty, magicStat, component.AncientBook);

                if (!success)
                {
                    _activities.RemoveActivity(actor);
                    DecrementBookCharge(actor, component.AncientBook);
                }
            }
        }

        private void ReadingAncientBook_OnFinish(EntityUid activity, ActivityReadingAncientBookComponent component, OnActivityFinishEvent args)
        {
            var actor = args.Activity.Actor;

            _mes.Display(Loc.GetString("Elona.Read.Activity.Finish", ("reader", actor), ("book", component.AncientBook)), entity: actor);
            _identify.IdentifyItem(component.AncientBook, IdentifyState.Full);
            _mes.Display(Loc.GetString("Elona.Read.AncientBook.FinishedDecoding", ("book", component.AncientBook)));

            if (TryComp<AncientBookComponent>(component.AncientBook, out var ancientBook))
            {
                ancientBook.IsDecoded = true;
            }

            _charges.SetCharges(component.AncientBook, 0);
            _stacks.TryStackAtSamePos(component.AncientBook);
        }

        private void ReadingAncientBook_OnCleanup(EntityUid uid, ActivityReadingAncientBookComponent component, OnActivityCleanupEvent args)
        {
            _inUse.RemoveItemInUse(component.AncientBook);
        }
    }
}

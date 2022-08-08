using OpenNefia.Content.Maps;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Spells;
using OpenNefia.Content.Memory;
using OpenNefia.Content.Identify;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        [Dependency] private readonly ISpellbookSystem _spellbooks = default!;
        [Dependency] private readonly IChargedSystem _charges = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;

        private void Initialize_ReadingSpellbook()
        {
            SubscribeComponent<ActivityReadingSpellbookComponent, OnActivityStartEvent>(ReadingSpellbook_OnStart);
            SubscribeComponent<ActivityReadingSpellbookComponent, OnActivityPassTurnEvent>(ReadingSpellbook_OnPassTurn);
            SubscribeComponent<ActivityReadingSpellbookComponent, OnActivityFinishEvent>(ReadingSpellbook_OnFinish);
            SubscribeComponent<ActivityReadingSpellbookComponent, OnActivityCleanupEvent>(ReadingSpellbook_OnCleanup);
        }

        private void ReadingSpellbook_OnStart(EntityUid activity, ActivityReadingSpellbookComponent component, OnActivityStartEvent args)
        {
            if (!IsAlive(component.Spellbook))
            {
                args.Cancel();
                return;
            }

            _mes.Display(Loc.GetString("Elona.Read.Activity.Start", ("entity", args.Activity.Actor), ("item", component.Spellbook)));
            _inUse.SetItemInUse(args.Activity.Actor, component.Spellbook);
        }

        private void DecrementBookCharge(EntityUid reader, EntityUid book)
        {
            _charges.ModifyCharges(book, -1);
            if (!_charges.HasChargesRemaining(book))
            {
                _mes.Display(Loc.GetString("Elona.Read.Activity.FallsApart", ("book", book)), entity: reader);
                _stacks.Use(book, 1);
            }
        }

        private void ReadingSpellbook_OnPassTurn(EntityUid activity, ActivityReadingSpellbookComponent component, OnActivityPassTurnEvent args)
        {
            var actor = args.Activity.Actor;

            _skills.GainSkillExp(actor, Protos.Skill.Literacy, 15, 10, 100);

            if (TryComp<SpellbookComponent>(component.Spellbook, out var spellbook))
            {
                int difficulty = _protos.Index(spellbook.SpellID).Difficulty;

                if (_curseStates.IsBlessed(component.Spellbook))
                    difficulty = (int)(difficulty / 1.2);
                else if (_curseStates.IsCursed(component.Spellbook))
                    difficulty = (int)(difficulty * 1.5);

                var spellLevel = _spells.Level(actor, spellbook.SpellID);
                var success = _spellbooks.TryToReadSpellbook(actor, component.Spellbook, difficulty, spellLevel);

                if (!success)
                {
                    _activities.RemoveActivity(actor);
                    DecrementBookCharge(actor, component.Spellbook);
                }
            }
        }

        private int CalcGainedSpellStock(int memorizationLevel, int currentStock)
        {
            var baseStock = 100;
            return (_rand.Next(baseStock / 2 + 1) + baseStock / 2)
                * (90 + memorizationLevel + (memorizationLevel > 0 ? 1 : 0) * 20)
                / Math.Clamp((100 + currentStock) / 2, 50, 100)
                + 1;
        }

        private void ReadingSpellbook_OnFinish(EntityUid activity, ActivityReadingSpellbookComponent component, OnActivityFinishEvent args)
        {
            var actor = args.Activity.Actor;

            _mes.Display(Loc.GetString("Elona.Read.Activity.Finish", ("reader", actor), ("book", component.Spellbook)), entity: actor);

            if (TryComp<SpellbookComponent>(component.Spellbook, out var spellbook))
            {
                var memorizationLevel = _skills.Level(actor, Protos.Skill.Memorization);
                var currentStock = _spells.SpellStock(actor, spellbook.SpellID);
                int difficulty = _protos.Index(spellbook.SpellID).Difficulty;

                _spells.GainSpell(actor, spellbook.SpellID, CalcGainedSpellStock(memorizationLevel, currentStock));
                _skills.GainSkillExp(actor, Protos.Skill.Memorization, 10 + difficulty / 5);

                if (TryProtoID(component.Spellbook, out var spellbookID) && !_spellbooks.SpellbookReserveStates.ContainsKey(spellbookID.Value))
                {
                    _spellbooks.SpellbookReserveStates[spellbookID.Value] = SpellbookReserveState.NotReserved;
                }
            }

            _identify.Identify(component.Spellbook, IdentifyState.Name);

            DecrementBookCharge(actor, component.Spellbook);
        }

        private void ReadingSpellbook_OnCleanup(EntityUid uid, ActivityReadingSpellbookComponent component, OnActivityCleanupEvent args)
        {
            _inUse.RemoveItemInUse(args.Activity.Actor, component.Spellbook);
        }
    }
}

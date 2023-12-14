using OpenNefia.Content.Activity;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Damage;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public sealed class InstrumentSystem : EntitySystem
    {
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<InstrumentComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, InstrumentComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Perform", () => Perform(args.Source, args.Target)));
        }

        private TurnResult Perform(EntityUid performer, EntityUid instrument)
        {
            if (!_damage.DoStaminaCheck(performer, 25, relatedSkillId: Protos.Skill.AttrCharisma))
            {
                _mes.Display(Loc.GetString("Elona.Common.TooExhausted"));
                return TurnResult.Failed;
            }

            var activity = EntityManager.SpawnEntity(Protos.Activity.Performing, MapCoordinates.Global);
            Comp<ActivityPerformingComponent>(activity).Instrument = instrument;
            _activities.StartActivity(performer, activity);
            return TurnResult.Succeeded;
        }
    }
}
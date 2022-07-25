using OpenNefia.Content.Activity;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public sealed class InstrumentSystem : EntitySystem
    {
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeComponent<InstrumentComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, InstrumentComponent component, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Perform", () => Perform(args.Source, args.Target)));
        }

        private TurnResult Perform(EntityUid performer, EntityUid instrument)
        {
            var activity = EntityManager.SpawnEntity(Protos.Activity.Performing, MapCoordinates.Global);
            Comp<ActivityPerformingComponent>(activity).Instrument = instrument;
            _activities.StartActivity(performer, activity);
            return TurnResult.Succeeded;
        }
    }
}
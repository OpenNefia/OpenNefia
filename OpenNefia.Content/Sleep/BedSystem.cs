using OpenNefia.Content.Activity;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
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

namespace OpenNefia.Content.Sleep
{
    public interface IBedSystem : IEntitySystem
    {
        TurnResult UseBed(EntityUid user, EntityUid bed, BedComponent? bedComp = null);
    }

    public sealed class BedSystem : EntitySystem, IBedSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IWorldSystem _world = default!;

        public override void Initialize()
        {
            SubscribeComponent<BedComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, BedComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(UseInventoryBehavior.VerbTypeUse, "Use Bed", () => UseBed(args.Source, args.Target, component)));
        }

        public TurnResult UseBed(EntityUid user, EntityUid bed, BedComponent? bedComp = null)
        {
            if (!Resolve(bed, ref bedComp))
                return TurnResult.Aborted;

            if (_world.State.AwakeTime < SleepSystem.SleepThresholdLight)
            {
                _mes.Display(Loc.GetString("Elona.Sleep.NotSleepy"));
                return TurnResult.Aborted;
            }

            var activity = EntityManager.SpawnEntity(Protos.Activity.PreparingToSleep, MapCoordinates.Global);
            Comp<ActivityPreparingToSleepComponent>(activity).Bed = bed;
            _activities.StartActivity(user, activity);

            return TurnResult.Succeeded;
        }
    }
}
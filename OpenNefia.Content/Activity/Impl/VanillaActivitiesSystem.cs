using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Activity.Impl
{
    public sealed class VanillaActivitiesSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            Initialize_Resting();
        }

        private void Initialize_Resting()
        {
            SubscribeComponent<ActivityRestingComponent, OnActivityStartEvent>(Resting_OnStart);
            SubscribeComponent<ActivityRestingComponent, OnActivityPassTurnEvent>(Resting_OnPassTurn);
            SubscribeComponent<ActivityRestingComponent, OnActivityFinishEvent>(Resting_OnFinish);
        }

        private void Resting_OnStart(EntityUid uid, ActivityRestingComponent component, OnActivityStartEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Activity.Resting.Start"));
        }

        private const int SleepThresholdHoursLight = 15;
        private const int SleepThresholdHoursModerate = 30;
        private const int SleepThresholdHoursHeavy = 50;

        private void Resting_OnPassTurn(EntityUid uid, ActivityRestingComponent component, OnActivityPassTurnEvent args)
        {
            if (!TryComp<ActivityComponent>(uid, out var activity))
                return;

            var actor = activity.Actor;

            if (activity.TurnsRemaining % 2 == 0)
            {
                _damage.HealStamina(actor, 1, showMessage: false);
            }
            if (activity.TurnsRemaining % 3 == 0)
            {
                _damage.HealHP(actor, 1, showMessage: false);
                _damage.HealMP(actor, 1, showMessage: false);
            }

            if (_world.State.AwakeTime.TotalHours >= SleepThresholdHoursModerate)
            {
                var doSleep = false;
                if (_world.State.AwakeTime.TotalHours >= SleepThresholdHoursHeavy || _rand.OneIn(2))
                {
                    doSleep = true;
                }

                if (doSleep)
                {
                    // TODO sleep
                    _mes.Display(Loc.GetString("Elona.Activity.Resting.DropOffToSleep"));
                    _activities.RemoveActivity(actor);
                }
            }
        }

        private void Resting_OnFinish(EntityUid uid, ActivityRestingComponent component, OnActivityFinishEvent args)
        {
            _mes.Display(Loc.GetString("Elona.Activity.Resting.Finish"));
        }
    }
}
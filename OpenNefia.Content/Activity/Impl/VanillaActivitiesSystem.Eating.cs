using OpenNefia.Content.Sleep;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Hunger;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Activity
{
    public sealed partial class VanillaActivitiesSystem
    {
        private void Initialize_Eating()
        {
            SubscribeComponent<ActivityEatingComponent, OnActivityStartEvent>(Eating_OnStart);
            SubscribeComponent<ActivityEatingComponent, OnActivityPassTurnEvent>(Eating_OnPassTurn);
            SubscribeComponent<ActivityEatingComponent, OnActivityFinishEvent>(Eating_OnFinish);
            SubscribeComponent<ActivityEatingComponent, OnActivityCleanupEvent>(Eating_OnCleanup);
        }

        private void Eating_OnStart(EntityUid activity, ActivityEatingComponent component, OnActivityStartEvent args)
        {
            var activityComp = args.Activity;
            var actor = activityComp.Actor;

            if (!IsAlive(component.Food))
            {
                _activities.RemoveActivity(actor);
                return;
            }

            if (_vis.IsInWindowFov(actor))
            {
                _audio.Play(Protos.Sound.Eat1, entityUid: actor);
                if (TryComp<PickableComponent>(component.Food.Value, out var pickable) && pickable.OwnState == OwnState.NPC && _parties.IsUnderlingOfPlayer(actor))
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Eating.Start.InSecret", ("actor", actor), ("food", component.Food.Value)));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Activity.Eating.Start.Normal", ("actor", actor), ("food", component.Food.Value)));
                }
            }

            _inUse.SetItemInUse(actor, component.Food.Value);

            var ev = new BeforeItemEatenEvent(actor);
            RaiseEvent(component.Food.Value, ev);
        }

        private void Eating_OnPassTurn(EntityUid activity, ActivityEatingComponent component, OnActivityPassTurnEvent args)
        {
            var activityComp = args.Activity;
            var actor = activityComp.Actor;

            if (!IsAlive(component.Food))
            {
                if (component.Food != null)
                    _inUse.RemoveItemInUse(actor, component.Food.Value);
                _activities.RemoveActivity(actor);
                return;
            }

            _emoIcons.SetEmotionIcon(actor, EmotionIcons.Eat);
            _inUse.SetItemInUse(actor, component.Food.Value);
        }

        private void Eating_OnFinish(EntityUid activity, ActivityEatingComponent component, OnActivityFinishEvent args)
        {
            var activityComp = args.Activity;
            var actor = activityComp.Actor;

            if (!IsAlive(component.Food))
            {
                if (component.Food != null)
                    _inUse.RemoveItemInUse(actor, component.Food.Value);
                _activities.RemoveActivity(actor);
                return;
            }

            if (component.ShowMessage)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Eating.Finish", ("actor", actor), ("food", component.Food)), entity: actor);
            }

            _inUse.RemoveItemInUse(actor, component.Food.Value);

            var ev = new AfterItemEatenEvent(actor);
            RaiseEvent(component.Food.Value, ev);
            EntityManager.DeleteEntity(component.Food.Value);
        }

        private void Eating_OnCleanup(EntityUid activity, ActivityEatingComponent component, OnActivityCleanupEvent args)
        {
            if (component.Food != null)
                _inUse.RemoveItemInUse(args.Activity.Actor, component.Food.Value);
        }
    }

    public sealed class BeforeItemEatenEvent : EntityEventArgs
    {
        public EntityUid Eater { get; }

        public BeforeItemEatenEvent(EntityUid eater)
        {
            Eater = eater;
        }
    }

    public sealed class AfterItemEatenEvent : EntityEventArgs
    {
        public EntityUid Eater { get; }

        public AfterItemEatenEvent(EntityUid eater)
        {
            Eater = eater;
        }
    }
}

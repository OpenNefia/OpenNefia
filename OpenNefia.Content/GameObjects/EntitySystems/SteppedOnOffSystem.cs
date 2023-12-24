using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class SteppedOnOffSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeComponent<MoveableComponent, BeforeMoveEventArgs>(HandleBeforeMove);
            SubscribeComponent<MoveableComponent, AfterMoveEventArgs>(HandleAfterMove);
        }

        private void HandleBeforeMove(EntityUid uid, MoveableComponent moveable, BeforeMoveEventArgs args)
        {
            CheckSteppedOff(uid, args, moveable);
        }

        private void HandleAfterMove(EntityUid uid, MoveableComponent moveable, AfterMoveEventArgs args)
        {
            CheckSteppedOn(uid, args, moveable);
        }

        private void CheckSteppedOff(EntityUid stepper,
            BeforeMoveEventArgs args,
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(stepper, ref moveable, ref spatial))
                return;

            foreach (var steppedSpatial in _lookup.GetLiveEntitiesAtCoords(spatial.MapPosition).ToList())
            {
                var ev = new BeforeEntitySteppedOffEvent(stepper, spatial.Coordinates);
                RaiseEvent(steppedSpatial.Owner, ev);
                if (ev.Handled)
                {
                    args.Handle(ev.TurnResult);
                    return;
                }

                if (!EntityManager.IsAlive(stepper))
                {
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }
        }

        private void CheckSteppedOn(EntityUid stepper,
            AfterMoveEventArgs args,
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(stepper, ref moveable, ref spatial))
                return;

            foreach (var steppedSpatial in _lookup.GetLiveEntitiesAtCoords(spatial.MapPosition).ToList())
            {
                var ev = new AfterEntitySteppedOnEvent(stepper, spatial.Coordinates);
                RaiseEvent(steppedSpatial.Owner, ev);

                if (!EntityManager.IsAlive(stepper))
                    break;
            }
        }
    }

    [EventUsage(EventTarget.Normal)]
    public class AfterEntitySteppedOnEvent : HandledEntityEventArgs
    {
        public EntityUid Stepper;
        public EntityCoordinates Coords;

        public AfterEntitySteppedOnEvent(EntityUid stepper, EntityCoordinates coords)
        {
            Stepper = stepper;
            Coords = coords;
        }
    }

    [EventUsage(EventTarget.Normal)]
    public class BeforeEntitySteppedOffEvent : TurnResultEntityEventArgs
    {
        public EntityUid Stepper;
        public EntityCoordinates Coords;

        public BeforeEntitySteppedOffEvent(EntityUid stepper, EntityCoordinates coords)
        {
            Stepper = stepper;
            Coords = coords;
        }
    }
}

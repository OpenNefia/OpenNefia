using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class SteppedOnSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, AfterMoveEventArgs>(HandleAfterMove, nameof(HandleAfterMove));
        }

        private void HandleAfterMove(EntityUid uid, MoveableComponent moveable, AfterMoveEventArgs args)
        {
            CheckSteppedOn(uid, args, moveable);
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
                var ev = new EntitySteppedOnEvent(stepper, spatial.MapPosition);
                RaiseLocalEvent(steppedSpatial.Owner, ev);

                if (!EntityManager.IsAlive(stepper))
                    break;
            }
        }
    }

    public class EntitySteppedOnEvent : HandledEntityEventArgs
    {
        public EntityUid Stepper;
        public MapCoordinates Coords;

        public EntitySteppedOnEvent(EntityUid stepper, MapCoordinates coords)
        {
            Stepper = stepper;
            Coords = coords;
        }
    }
}

using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.GameObjects
{
    public class MovementSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, MoveEventArgs>(HandleMove);
            SubscribeLocalEvent<MoveableComponent, PositionChangedEvent>(HandlePositionChanged);
        }

        private void HandlePositionChanged(EntityUid uid, MoveableComponent component, ref PositionChangedEvent args)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            spatial.Map!.RefreshTile(args.OldPosition.Position);
            spatial.Map.RefreshTile(args.NewPosition.Position);
        }

        private void HandleMove(EntityUid uid, MoveableComponent moveable, MoveEventArgs args)
        {
            if (args.Handled || !EntityManager.IsAlive(uid))
                return;

            HandleMove(uid, args, moveable);
        }

        private void HandleMove(EntityUid uid, MoveEventArgs args, 
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(uid, ref moveable, ref spatial))
                return;

            spatial.Direction = (args.NewPosition.Position - args.OldPosition.Position).GetDir();

            var evBefore = new BeforeMoveEventArgs()
            {
                OldPosition = args.OldPosition,
                NewPosition = args.NewPosition
            };
            RaiseLocalEvent(uid, evBefore);

            if (evBefore.Handled || !EntityManager.IsAlive(uid))
            {
                args.Handled = true;
                args.TurnResult = evBefore.TurnResult;
                return;
            }

            if (args.OldPosition.Map != args.NewPosition.Map || !args.NewPosition.CanAccess())
            {
                args.Handled = true;
                args.TurnResult = TurnResult.Failed;
                return;
            }

            spatial.Pos = args.NewPosition.Position;

            var evAfter = new AfterMoveEventArgs()
            {
                OldPosition = args.OldPosition,
                NewPosition = args.NewPosition
            };
            RaiseLocalEvent(uid, evAfter);

            args.Handled = true;
            args.TurnResult = TurnResult.Succeeded;
        }
    }

    public struct PositionChangedEvent
    {
        public MapCoordinates OldPosition;
        public MapCoordinates NewPosition;
    }

    public class MoveEventArgs : HandledEntityEventArgs
    {
        public MapCoordinates OldPosition;
        public MapCoordinates NewPosition;

        public TurnResult TurnResult;
    }

    public class BeforeMoveEventArgs : HandledEntityEventArgs
    {
        public MapCoordinates OldPosition;
        public MapCoordinates NewPosition;

        public TurnResult TurnResult;
    }

    public class AfterMoveEventArgs : HandledEntityEventArgs
    {
        public MapCoordinates OldPosition;
        public MapCoordinates NewPosition;
    }
}

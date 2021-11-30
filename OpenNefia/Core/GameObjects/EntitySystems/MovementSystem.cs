using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.GameObjects
{
    public class MovementSystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, OnMoveEventArgs>(HandleOnMove);
        }

        private void HandleOnMove(EntityUid uid, MoveableComponent moveable, OnMoveEventArgs args)
        {
            if (args.Handled)
                return;

            OnMove(uid, args, moveable);
        }

        private void OnMove(EntityUid uid, OnMoveEventArgs args, 
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

            if (evBefore.Handled)
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
            spatial.Map!.RefreshTile(args.OldPosition.Position);
            spatial.Map.RefreshTile(args.NewPosition.Position);

            var evAfter = new AfterMoveEventArgs();
            RaiseLocalEvent(uid, evAfter);

            args.Handled = true;
            args.TurnResult = TurnResult.Succeeded;
        }
    }

    public class OnMoveEventArgs : HandledEntityEventArgs
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
    }
}

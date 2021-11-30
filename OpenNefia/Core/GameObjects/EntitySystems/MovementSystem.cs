using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.GameObjects.EntitySystems
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

            spatial.Direction = (args.NewPos.Position - args.OldPos.Position).GetDir();

            var evBefore = new BeforeMoveEventArgs()
            {
                OldPos = args.OldPos,
                NewPos = args.NewPos
            };
            RaiseLocalEvent(uid, evBefore);

            if (evBefore.Handled)
            {
                args.Handled = true;
                args.TurnResult = evBefore.TurnResult;
                return;
            }

            if (args.OldPos.Map != args.NewPos.Map || !args.NewPos.CanAccess())
            {
                args.Handled = true;
                args.TurnResult = TurnResult.Failed;
                return;
            }

            spatial.Pos = args.NewPos.Position;
            spatial.Map!.RefreshTile(args.OldPos.Position);
            spatial.Map.RefreshTile(args.NewPos.Position);

            var evAfter = new AfterMoveEventArgs();
            RaiseLocalEvent(uid, evAfter);

            args.Handled = true;
            args.TurnResult = TurnResult.Succeeded;
        }
    }

    public class OnMoveEventArgs : HandledEntityEventArgs
    {
        public MapCoordinates OldPos;
        public MapCoordinates NewPos;

        public TurnResult TurnResult;
    }

    public class BeforeMoveEventArgs : HandledEntityEventArgs
    {
        public MapCoordinates OldPos;
        public MapCoordinates NewPos;

        public TurnResult TurnResult;
    }

    public class AfterMoveEventArgs : HandledEntityEventArgs
    {
    }
}

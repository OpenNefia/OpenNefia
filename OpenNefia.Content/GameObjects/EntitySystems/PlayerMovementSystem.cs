using OpenNefia.Content.Logic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class PlayerMovementSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, BeforeMoveEventArgs>(HandleBeforeMove, nameof(HandleBeforeMove));
            SubscribeLocalEvent<PlayerComponent, BeforeMoveEventArgs>(HandleBeforeMovePlayer, nameof(HandleBeforeMovePlayer));
        }

        private void HandleBeforeMovePlayer(EntityUid uid, PlayerComponent player, BeforeMoveEventArgs args)
        {
            CheckMovementOutOfMap(uid, args, player);
        }

        public void CheckMovementOutOfMap(EntityUid uid, BeforeMoveEventArgs args,
           PlayerComponent? player = null)
        {
            if (args.Handled || !Resolve(uid, ref player))
                return;

            var (pos, mapId) = args.NewPosition;

            if (_mapManager.TryGetMap(mapId, out var map) && !map.IsInBounds(pos))
            {
                if (_playerQuery.YesOrNo("Do you want to exit the map?"))
                {
                    RaiseLocalEvent(uid, new ExitMapEventArgs());
                    args.Handle(TurnResult.Succeeded);
                    return;
                }
            }
        }

        private void HandleBeforeMove(EntityUid uid, MoveableComponent moveable, BeforeMoveEventArgs args)
        {
            CollideWithEntities(uid, args, moveable);
        }

        public void CollideWithEntities(EntityUid source, BeforeMoveEventArgs args,
            MoveableComponent? moveable = null)
        {
            if (args.Handled || !Resolve(source, ref moveable))
                return;

            var entities = _lookup.GetLiveEntitiesAtPos(args.NewPosition)
                .Where(x => x.Spatial.IsSolid);

            foreach (var collided in entities.ToList())
            {
                var ev = new CollideWithEventArgs(collided.Uid);
                if (!Raise(source, ev, args))
                    return;

                var ev2 = new WasCollidedWithEventArgs(source);
                if (!Raise(collided.Uid, ev2, args))
                    return;
            }
        }
    }

    public class ExitMapEventArgs
    {
        public ExitMapEventArgs()
        {
        }
    }

    public class CollideWithEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Target;

        public CollideWithEventArgs(EntityUid target)
        {
            Target = target;
        }
    }

    public class WasCollidedWithEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Source;

        public WasCollidedWithEventArgs(EntityUid source)
        {
            Source = source;
        }
    }
}

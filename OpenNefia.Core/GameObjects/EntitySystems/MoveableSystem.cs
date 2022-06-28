using OpenNefia.Core.Directions;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Handles moving moveable entities.
    /// </summary>
    public class MoveableSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<MoveableComponent, MoveEventArgs>(HandleMove);
        }

        #region Methods

        public TurnResult? MoveEntity(EntityUid entity, MapCoordinates newPosition, 
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref moveable, ref spatial))
                return null;

            var oldPosition = spatial.MapPosition;
            var ev = new MoveEventArgs(oldPosition, newPosition);
            RaiseLocalEvent(entity, ev);
            return ev.TurnResult;
        }

        #endregion

        #region Event Handlers

        private void HandleMove(EntityUid uid, MoveableComponent moveable, MoveEventArgs args)
        {
            if (args.Handled || !EntityManager.IsAlive(uid))
                return;

            var result = HandleMove(uid, args.NewPosition, args.OldPosition, moveable);

            if (result != null)
                args.Handle(result.Value);
        }

        private TurnResult? HandleMove(EntityUid uid, 
            MapCoordinates newCoords,
            MapCoordinates oldCoords,
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(uid, ref moveable, ref spatial))
                return null;

            if (newCoords == oldCoords)
                return TurnResult.Succeeded;

            if (oldCoords.TryDirectionTowards(newCoords, out var newDir))
                spatial.Direction = newDir;

            var evBefore = new BeforeMoveEventArgs(oldCoords, newCoords);

            if (Raise(uid, evBefore))
                return evBefore.TurnResult;

            if (!_mapManager.TryGetMap(newCoords.MapId, out var map)
                || !map.CanAccess(newCoords.Position))
            {
                return TurnResult.Failed;
            }

            spatial.WorldPosition = newCoords.Position;

            var evAfter = new AfterMoveEventArgs(oldCoords, newCoords);
            RaiseLocalEvent(uid, evAfter);

            return TurnResult.Succeeded;
        }

        public bool SwapPlaces(EntityUid entity, EntityUid with,
            SpatialComponent? spatial = null,
            SpatialComponent? withSpatial = null)
        {
            if (!Resolve(entity, ref spatial) || !Resolve(with, ref withSpatial))
                return false;

            var temp = spatial.WorldPosition;
            spatial.WorldPosition = withSpatial.WorldPosition;
            withSpatial.WorldPosition = temp;

            return true;
        }

        #endregion
    }

    public class MoveEventArgs : TurnResultEntityEventArgs
    {
        public readonly MapCoordinates OldPosition;
        public readonly MapCoordinates NewPosition;

        public MoveEventArgs(MapCoordinates oldPosition, MapCoordinates newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }

    public class BeforeMoveEventArgs : TurnResultEntityEventArgs
    {
        public readonly MapCoordinates OldPosition;
        public readonly MapCoordinates NewPosition;

        public BeforeMoveEventArgs(MapCoordinates oldPosition, MapCoordinates newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }

    public class AfterMoveEventArgs : HandledEntityEventArgs
    {
        public readonly MapCoordinates OldPosition;
        public readonly MapCoordinates NewPosition;

        public AfterMoveEventArgs(MapCoordinates oldPosition, MapCoordinates newPosition)
        {
            OldPosition = oldPosition;
            NewPosition = newPosition;
        }
    }
}

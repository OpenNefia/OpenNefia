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
    public interface IMoveableSystem : IEntitySystem
    {
        /// <summary>
        /// Moves this entity, raising additional entity events.
        /// 
        /// Most entities' positions can be changed by assigning the <see
        /// cref="SpatialComponent.Coordinates"/> property, but this method will also run the three
        /// movement events for Moveable entities:
        /// 
        /// <list type="bullet">
        /// <item><see cref="MoveEventArgs"/></item>
        /// <item><see cref="BeforeMoveEventArgs"/></item>
        /// <item><see cref="AfterMoveEventArgs"/></item>
        /// </list>
        /// 
        /// Use this method when trying to move an NPC during their AI routine.
        /// </summary>
        TurnResult? MoveEntity(EntityUid entity, MapCoordinates newPosition, MoveableComponent? moveable = null, SpatialComponent? spatial = null);

        bool SwapPlaces(EntityUid entity, EntityUid with, SpatialComponent? spatial = null, SpatialComponent? withSpatial = null);
    }

    /// <summary>
    /// Handles moving moveable entities.
    /// </summary>
    public class MoveableSystem : EntitySystem, IMoveableSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<MoveableComponent, MoveEventArgs>(HandleMove);
        }

        #region Methods

        /// <inheritdoc/>
        public TurnResult? MoveEntity(EntityUid entity, MapCoordinates newPosition,
            MoveableComponent? moveable = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref moveable, ref spatial))
                return null;

            var oldPosition = spatial.MapPosition;
            var ev = new MoveEventArgs(oldPosition, newPosition);
            RaiseEvent(entity, ev);
            return ev.TurnResult;
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

            newCoords = evBefore.OutNewPosition;

            if (!_mapManager.TryGetMap(newCoords.MapId, out var map)
                || !map.CanAccess(newCoords.Position))
            {
                return TurnResult.Failed;
            }

            spatial.WorldPosition = newCoords.Position;

            var evAfter = new AfterMoveEventArgs(oldCoords, newCoords);
            RaiseEvent(uid, evAfter);

            return TurnResult.Succeeded;
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
        public readonly MapCoordinates DesiredPosition;

        public MapCoordinates OutNewPosition;

        public BeforeMoveEventArgs(MapCoordinates oldPosition, MapCoordinates newPosition)
        {
            OldPosition = oldPosition;
            DesiredPosition = newPosition;
            OutNewPosition = DesiredPosition;
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

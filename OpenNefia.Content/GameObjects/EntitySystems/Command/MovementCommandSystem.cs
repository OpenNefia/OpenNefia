using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class MovementCommandSystem : EntitySystem
    {
        [Dependency] private readonly MoveableSystem _moveable = default!;

        public override void Initialize()
        {
            var moveNorthCmdHandler = new MoverDirInputCmdHandler(Direction.North);
            var moveSouthCmdHandler = new MoverDirInputCmdHandler(Direction.South);
            var moveWestCmdHandler = new MoverDirInputCmdHandler(Direction.West);
            var moveEastCmdHandler = new MoverDirInputCmdHandler(Direction.East);
            var moveNorthEastCmdHandler = new MoverDirInputCmdHandler(Direction.NorthEast);
            var moveSouthEastCmdHandler = new MoverDirInputCmdHandler(Direction.SouthEast);
            var moveSouthWestCmdHandler = new MoverDirInputCmdHandler(Direction.SouthWest);
            var moveNorthWestCmdHandler = new MoverDirInputCmdHandler(Direction.NorthWest);

            CommandBinds.Builder
                .Bind(EngineKeyFunctions.North, moveNorthCmdHandler)
                .Bind(EngineKeyFunctions.West, moveWestCmdHandler)
                .Bind(EngineKeyFunctions.East, moveEastCmdHandler)
                .Bind(EngineKeyFunctions.South, moveSouthCmdHandler)
                .Bind(EngineKeyFunctions.Northeast, moveNorthEastCmdHandler)
                .Bind(EngineKeyFunctions.Southeast, moveSouthEastCmdHandler)
                .Bind(EngineKeyFunctions.Southwest, moveSouthWestCmdHandler)
                .Bind(EngineKeyFunctions.Northwest, moveNorthWestCmdHandler)
                .Bind(EngineKeyFunctions.Wait, InputCmdHandler.FromDelegate(HandleWait))
                .Register<MovementCommandSystem>();
        }

        private TurnResult? HandleMove(EntityUid entity, Direction dir,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref spatial))
                return null;

            var newPosition = spatial.MapPosition.Offset(dir.ToIntVec());
            return _moveable.MoveEntity(entity, newPosition, spatial: spatial);
        }

        private TurnResult? HandleWait(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            return HandleWait(session.Player.Uid);
        }

        private TurnResult? HandleWait(EntityUid entity,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entity, ref spatial))
                return null;

            var newPosition = spatial.MapPosition;
            return _moveable.MoveEntity(entity, newPosition, spatial: spatial);
        }

        private sealed class MoverDirInputCmdHandler : InputCmdHandler
        {
            private readonly Direction _dir;

            public MoverDirInputCmdHandler(Direction dir)
            {
                _dir = dir;
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage full || session?.Player == null)
                {
                    return null;
                }

                if (full.State == BoundKeyState.Down)
                {
                    return Get<MovementCommandSystem>().HandleMove(session.Player.Uid, _dir);
                }
                return null;
            }
        }
    }
}

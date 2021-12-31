﻿using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
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
        public override void Initialize()
        {
            var moveNorthCmdHandler = new MoverDirInputCmdHandler(Direction.North);
            var moveSouthCmdHandler = new MoverDirInputCmdHandler(Direction.South);
            var moveWestCmdHandler = new MoverDirInputCmdHandler(Direction.West);
            var moveEastCmdHandler = new MoverDirInputCmdHandler(Direction.East);

            CommandBinds.Builder
                .Bind(EngineKeyFunctions.North, moveNorthCmdHandler)
                .Bind(EngineKeyFunctions.West, moveWestCmdHandler)
                .Bind(EngineKeyFunctions.East, moveEastCmdHandler)
                .Bind(EngineKeyFunctions.South, moveSouthCmdHandler)
                .Register<MovementCommandSystem>();
        }

        private TurnResult? HandleMove(IGameSessionManager? session, Direction dir)
        {
            var player = session?.Player;
            if (player == null)
                return null;

            var oldPosition = player.Spatial.MapPosition;
            var newPosition = player.Spatial.MapPosition.Offset(dir.ToIntVec());
            var ev = new MoveEventArgs(oldPosition, newPosition);
            player.EntityManager.EventBus.RaiseLocalEvent(player.Uid, ev);
            return ev.TurnResult;
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
                if (message is not FullInputCmdMessage full)
                {
                    return null;
                }

                if (full.State == BoundKeyState.Down)
                {
                    return Get<MovementCommandSystem>().HandleMove(session, _dir);
                }
                return null;
            }
        }
    }
}

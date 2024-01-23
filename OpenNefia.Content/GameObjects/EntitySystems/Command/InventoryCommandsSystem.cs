﻿using OpenNefia.Content.Equipment;
using OpenNefia.Content.Input;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    /// <summary>
    /// System for commands that open the inventory.
    /// </summary>
    public class InventoryCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVerbSystem _verbSystem = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IUserInterfaceManager _uiMgr = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ICommonCommandsSystem _cmd = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(ContentKeyFunctions.PickUp, _cmd.MakeCommandHandler(HandlePickUp))
                .Bind(ContentKeyFunctions.Examine,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new ExamineInventoryBehavior()))
                .Bind(ContentKeyFunctions.Drop,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new DropInventoryBehavior()))
                .Bind(ContentKeyFunctions.Drink,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new DrinkInventoryBehavior()))
                .Bind(ContentKeyFunctions.Throw,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new ThrowInventoryBehavior()))
                .Bind(ContentKeyFunctions.Eat,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new EatInventoryBehavior()))
                .Bind(ContentKeyFunctions.Use,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new UseInventoryBehavior()))
                .Bind(ContentKeyFunctions.Read,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new ReadInventoryBehavior()))
                .Bind(ContentKeyFunctions.Open,
                    new InventoryInputCmdHandler(_uiMgr, _cmd, new OpenInventoryBehavior()))
                .Register<InventoryCommandsSystem>();
        }

        private TurnResult? HandlePickUp(IGameSessionManager? session)
        {
            if (session == null)
                return null;

            var player = session.Player;

            var verbReq = new VerbRequest(PickableSystem.VerbTypePickUp);
            var spatials = _lookup.EntitiesUnderneath(session.Player)
                .Select(spatial => (spatial, _verbSystem.GetVerbOrNull(session.Player, spatial.Owner, verbReq)))
                .Where(pair => pair.Item2 != null)
                .ToList();

            if (spatials.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.GraspAtAir"));
                return TurnResult.Failed;
            }
            else if (spatials.Count == 1)
            {
                var (spatial, verb) = spatials.First();
                var result = verb!.Act();

                if (result == TurnResult.NoResult)
                {
                    // Try map-specific actions, like gathering snow.
                    var map = _mapManager.GetMap(spatial.MapID);
                    if (_verbSystem.TryGetVerb(player, map.MapEntityUid, verbReq, out verb))
                        result = verb.Act();
                }

                if (result == TurnResult.NoResult)
                {
                    _mes.Display(Loc.GetString("Elona.GameObjects.Pickable.GraspAtAir"));
                    result = TurnResult.Failed;
                }

                return result;
            }
            else
            {
                var context = new InventoryContext(player, new PickUpInventoryBehavior());
                var result = _uiMgr.Query<InventoryLayer, InventoryContext, InventoryLayer.Result>(context);

                if (result.HasValue && result.Value.Data is InventoryResult.Finished finished)
                {
                    return finished.TurnResult;
                }

                return TurnResult.Aborted;
            }
        }

        private sealed class InventoryInputCmdHandler : InputCmdHandler
        {
            private readonly IUserInterfaceManager _uiMgr;
            private readonly ICommonCommandsSystem _cmd;
            private readonly IInventoryBehavior _behavior;

            private static readonly List<IInventoryBehavior> DefaultBehaviors = new List<IInventoryBehavior>
            {
                new ExamineInventoryBehavior(),
                new DropInventoryBehavior(),
                new EatInventoryBehavior(),
                new ReadInventoryBehavior(),
                new DrinkInventoryBehavior(),
                // TODO: zap behaviour
                new UseInventoryBehavior(),
                new OpenInventoryBehavior(),
                // TODO: mix behaviour
                new ThrowInventoryBehavior(),
            };


            public InventoryInputCmdHandler(IUserInterfaceManager uiMgr, ICommonCommandsSystem cmd,
                IInventoryBehavior behavior)
            {
                _behavior = behavior;
                _uiMgr = uiMgr;
                _cmd = cmd;
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage full)
                {
                    return null;
                }

                var behaviors = DefaultBehaviors;
                var index = behaviors.FindIndex(b => b.GetType() == _behavior.GetType());
                if (index == -1)
                {
                    behaviors = new List<IInventoryBehavior>() { _behavior };
                    index = 0;
                }

                if (_behavior.BlockInWorldMap && _cmd.BlockIfInWorldMap(session!.Player))
                    return TurnResult.Aborted;

                if (full.State == BoundKeyState.Down && session?.Player != null)
                {
                    var context = new InventoryGroupArgs(session.Player, behaviors, index);
                    var result = _uiMgr.Query<InventoryUiGroup, InventoryGroupArgs, InventoryLayer.Result>(context);

                    if (result.HasValue)
                    {
                        switch (result.Value.Data)
                        {
                            case InventoryResult.Finished finished:
                                return finished.TurnResult;
                            default:
                                return TurnResult.Aborted;
                        }
                    }
                    else
                    {
                        return TurnResult.Aborted;
                    }
                }
                return null;
            }
        }
    }
}
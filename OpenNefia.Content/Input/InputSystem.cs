using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Input
{
    public interface IInputSystem : IEntitySystem
    {
        bool PlayerIsRunning();
    }

    /// <summary>
    /// Dispatches bound key events to systems that have registered input command handlers.
    /// </summary>
    public sealed class InputSystem : EntitySystem, IInputSystem
    {
        [Dependency] protected readonly IInputManager _inputManager = default!;
        [Dependency] protected readonly IEntitySystemManager _entitySystemManager = default!;
        [Dependency] protected readonly IGameSessionManager _gameSession = default!;
        [Dependency] protected readonly IFieldLayer _field = default!;
        [Dependency] protected readonly ITurnOrderSystem _turnOrder = default!;

        private readonly IPlayerCommandStates _cmdStates = new PlayerCommandStates();

        /// <summary>
        ///     Current states for all of the keyFunctions.
        /// </summary>
        public IPlayerCommandStates CmdStates => _cmdStates;

        public override void Initialize()
        {
            _inputManager.KeyBindStateChanged += OnKeyBindStateChanged;
        }

        public bool PlayerIsRunning()
        {
            // TODO
            return false;
        }

        /// <summary>
        ///     Converts a state change event from outside the simulation to inside the simulation.
        /// </summary>
        /// <param name="args">Event data values for a bound key state change.</param>
        private void OnKeyBindStateChanged(ViewportBoundKeyEventArgs args)
        {
            // Doesn't make sense to run simulation keybinds if there's an active modal.
            if (!_field.IsQuerying())
            {
                return;
            }

            var kArgs = args.KeyEventArgs;
            var func = kArgs.Function;
            var funcId = _inputManager.NetworkBindMap.KeyFunctionID(func);

            EntityCoordinates coordinates = default;
            EntityUid? entityToClick = null;

            var message = new FullInputCmdMessage(funcId, kArgs.State,
                coordinates, kArgs.PointerLocation,
                entityToClick);

            if (HandleInputCommand(_gameSession, func, message))
            {
                kArgs.Handle();
            }
        }

        /// <summary>
        ///     Inserts an Input Command into the simulation.
        /// </summary>
        /// <param name="session">Player session that raised the command.</param>
        /// <param name="function">Function that is being changed.</param>
        /// <param name="message">Arguments for this event.</param>
        public bool HandleInputCommand(IGameSessionManager? session, BoundKeyFunction function, FullInputCmdMessage message)
        {
#if DEBUG
            var funcId = _inputManager.NetworkBindMap.KeyFunctionID(function);
            DebugTools.Assert(funcId == message.InputFunctionId, "Function ID in message does not match function.");
#endif

            //if (_cmdStates.GetState(function) == message.State)
            //{
            //    return false;
            //}
            _cmdStates.SetState(function, message.State);

            foreach (var handler in _inputManager.BindRegistry.GetHandlers(function))
            {
                var result = handler.HandleCmdMessage(session, message);
                if (result != null)
                {
                    _turnOrder.AdvanceStateFromPlayer(result.Value);
                    return true;
                }
            }

            return false;
        }

    }
}

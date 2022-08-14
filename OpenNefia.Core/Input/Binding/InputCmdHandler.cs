using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.Input.Binding
{
    public delegate TurnResult? StateInputCmdDelegate(IGameSessionManager? session);

    public abstract class InputCmdHandler
    {
        public virtual TurnResult? Enabled(IGameSessionManager? session)
        {
            return null;
        }

        public virtual TurnResult? Disabled(IGameSessionManager? session)
        {
            return null;
        }

        public abstract TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message);

        /// <summary>
        ///     Makes a quick input command from enabled and disabled delegates.
        /// </summary>
        /// <param name="enabled">The delegate to be ran when this command is enabled.</param>
        /// <param name="disabled">The delegate to be ran when this command is disabled.</param>
        /// <returns>The new input command.</returns>
        public static InputCmdHandler FromDelegate(StateInputCmdDelegate? enabled = null,
            StateInputCmdDelegate? disabled = null)
        {
            return new StateInputCmdHandler
            {
                EnabledDelegate = enabled,
                DisabledDelegate = disabled,
            };
        }

        private class StateInputCmdHandler : InputCmdHandler
        {
            public StateInputCmdDelegate? EnabledDelegate;
            public StateInputCmdDelegate? DisabledDelegate;

            public override TurnResult? Enabled(IGameSessionManager? session)
            {
                return EnabledDelegate?.Invoke(session);
            }

            public override TurnResult? Disabled(IGameSessionManager? session)
            {
                return DisabledDelegate?.Invoke(session);
            }

            public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (message is not FullInputCmdMessage msg)
                    return null;

                switch (msg.State)
                {
                    case BoundKeyState.Up:
                        return Disabled(session);
                    case BoundKeyState.Down:
                        return Enabled(session);
                }

                //Client Sanitization: unknown key state, just ignore
                return null;
            }
        }
    }

    public delegate TurnResult? PointerInputCmdDelegate(IGameSessionManager? session, EntityCoordinates coords, EntityUid? uid);

    public delegate TurnResult? PointerInputCmdDelegate2(in PointerInputCmdHandler.PointerInputCmdArgs args);

    public class PointerInputCmdHandler : InputCmdHandler
    {
        private PointerInputCmdDelegate2 _callback;
        private bool _ignoreUp;

        /// <summary>
        /// Handler which will handle the command using the indicated callback
        /// </summary>
        /// <param name="callback">callback to handle the command</param>
        /// <param name="ignoreUp">whether keyup actions will be ignored by this handler (like lifting a key or releasing
        /// mouse button)</param>
        public PointerInputCmdHandler(PointerInputCmdDelegate callback, bool ignoreUp = true)
            : this((in PointerInputCmdArgs args) =>
            callback(args.Session, args.Coordinates, args.EntityUid), ignoreUp)
        { }

        /// <summary>
        /// Handler which will handle the command using the indicated callback
        /// </summary>
        /// <param name="callback">callback to handle the command</param>
        /// <param name="ignoreUp">whether keyup actions will be ignored by this handler (like lifting a key or releasing
        /// mouse button)</param>
        public PointerInputCmdHandler(PointerInputCmdDelegate2 callback, bool ignoreUp = true)
        {
            _callback = callback;
            _ignoreUp = ignoreUp;

        }

        public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg) || (_ignoreUp && msg.State != BoundKeyState.Down))
                return null;

            return _callback?.Invoke(new PointerInputCmdArgs(session, msg.Coordinates,
                msg.ScreenCoordinates, msg.Uid, msg.State, msg));
        }

        public readonly struct PointerInputCmdArgs
        {
            public readonly IGameSessionManager? Session;
            public readonly EntityCoordinates Coordinates;
            public readonly ScreenCoordinates ScreenCoordinates;
            public readonly EntityUid? EntityUid;
            public readonly BoundKeyState State;
            public readonly FullInputCmdMessage OriginalMessage;

            public PointerInputCmdArgs(IGameSessionManager? session, EntityCoordinates coordinates,
                ScreenCoordinates screenCoordinates, EntityUid? entityUid, BoundKeyState state,
                FullInputCmdMessage originalMessage)
            {
                Session = session;
                Coordinates = coordinates;
                ScreenCoordinates = screenCoordinates;
                EntityUid = entityUid;
                State = state;
                OriginalMessage = originalMessage;
            }
        }
    }

    public class PointerStateInputCmdHandler : InputCmdHandler
    {
        private PointerInputCmdDelegate _enabled;
        private PointerInputCmdDelegate _disabled;

        public PointerStateInputCmdHandler(PointerInputCmdDelegate enabled, PointerInputCmdDelegate disabled)
        {
            _enabled = enabled;
            _disabled = disabled;
        }

        /// <inheritdoc />
        public override TurnResult? HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg))
                return null;

            switch (msg.State)
            {
                case BoundKeyState.Up:
                    return _disabled?.Invoke(session, msg.Coordinates, msg.Uid);
                case BoundKeyState.Down:
                    return _enabled?.Invoke(session, msg.Coordinates, msg.Uid);
            }

            //Client Sanitization: unknown key state, just ignore
            return null;
        }
    }
}

using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.Input.Binding
{
    public delegate void StateInputCmdDelegate(IGameSessionManager? session);

    public abstract class InputCmdHandler
    {
        public virtual void Enabled(IGameSessionManager? session)
        {
        }

        public virtual void Disabled(IGameSessionManager? session)
        {
        }

        public abstract bool HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message);

        /// <summary>
        ///     Makes a quick input command from enabled and disabled delegates.
        /// </summary>
        /// <param name="enabled">The delegate to be ran when this command is enabled.</param>
        /// <param name="disabled">The delegate to be ran when this command is disabled.</param>
        /// <returns>The new input command.</returns>
        public static InputCmdHandler FromDelegate(StateInputCmdDelegate? enabled = null,
            StateInputCmdDelegate? disabled = null, bool handle=true)
        {
            return new StateInputCmdHandler
            {
                EnabledDelegate = enabled,
                DisabledDelegate = disabled,
                Handle = handle,
            };
        }

        private class StateInputCmdHandler : InputCmdHandler
        {
            public StateInputCmdDelegate? EnabledDelegate;
            public StateInputCmdDelegate? DisabledDelegate;
            public bool Handle { get; set; }

            public override void Enabled(IGameSessionManager? session)
            {
                EnabledDelegate?.Invoke(session);
            }

            public override void Disabled(IGameSessionManager? session)
            {
                DisabledDelegate?.Invoke(session);
            }

            public override bool HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
            {
                if (!(message is FullInputCmdMessage msg))
                    return false;

                switch (msg.State)
                {
                    case BoundKeyState.Up:
                        Disabled(session);
                        return Handle;
                    case BoundKeyState.Down:
                        Enabled(session);
                        return Handle;
                }

                //Client Sanitization: unknown key state, just ignore
                return false;
            }
        }
    }

    public delegate bool PointerInputCmdDelegate(IGameSessionManager? session, EntityCoordinates coords, EntityUid? uid);

    public delegate bool PointerInputCmdDelegate2(in PointerInputCmdHandler.PointerInputCmdArgs args);

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
            callback(args.Session, args.Coordinates, args.EntityUid), ignoreUp) { }

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

        public override bool HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg) || (_ignoreUp && msg.State != BoundKeyState.Down))
                return false;

            var handled = _callback?.Invoke(new PointerInputCmdArgs(session, msg.Coordinates,
                msg.ScreenCoordinates, msg.Uid, msg.State, msg));
            return handled.HasValue && handled.Value;
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
        public override bool HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg))
                return false;

            switch (msg.State)
            {
                case BoundKeyState.Up:
                    return _disabled?.Invoke(session, msg.Coordinates, msg.Uid) == true;
                case BoundKeyState.Down:
                    return _enabled?.Invoke(session, msg.Coordinates, msg.Uid) == true;
            }

            //Client Sanitization: unknown key state, just ignore
            return false;
        }
    }

    /// <summary>
    /// Consumes both up and down states without calling any handler delegates. Primarily used on the client to
    /// prevent an input message from being sent to the server.
    /// </summary>
    public class NullInputCmdHandler : InputCmdHandler
    {
        /// <inheritdoc />
        public override bool HandleCmdMessage(IGameSessionManager? session, InputCmdMessage message)
        {
            return true;
        }
    }
}

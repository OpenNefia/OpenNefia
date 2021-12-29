using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.Input.Binding
{
    public delegate void StateInputCmdDelegate();

    public abstract class InputCmdHandler
    {
        public virtual void Enabled()
        {
        }

        public virtual void Disabled()
        {
        }

        public abstract bool HandleCmdMessage(InputCmdMessage message);

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

            public override void Enabled()
            {
                EnabledDelegate?.Invoke();
            }

            public override void Disabled()
            {
                DisabledDelegate?.Invoke();
            }

            public override bool HandleCmdMessage(InputCmdMessage message)
            {
                if (!(message is FullInputCmdMessage msg))
                    return false;

                switch (msg.State)
                {
                    case BoundKeyState.Up:
                        Disabled();
                        return Handle;
                    case BoundKeyState.Down:
                        Enabled();
                        return Handle;
                }

                //Client Sanitization: unknown key state, just ignore
                return false;
            }
        }
    }

    public delegate bool PointerInputCmdDelegate(EntityCoordinates coords, EntityUid uid);

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
            callback(args.Coordinates, args.EntityUid), ignoreUp) { }

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

        public override bool HandleCmdMessage(InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg) || (_ignoreUp && msg.State != BoundKeyState.Down))
                return false;

            var handled = _callback?.Invoke(new PointerInputCmdArgs(msg.Coordinates,
                msg.ScreenCoordinates, msg.Uid, msg.State, msg));
            return handled.HasValue && handled.Value;
        }

        public readonly struct PointerInputCmdArgs
        {
            public readonly EntityCoordinates Coordinates;
            public readonly ScreenCoordinates ScreenCoordinates;
            public readonly EntityUid EntityUid;
            public readonly BoundKeyState State;
            public readonly FullInputCmdMessage OriginalMessage;

            public PointerInputCmdArgs(EntityCoordinates coordinates,
                ScreenCoordinates screenCoordinates, EntityUid entityUid, BoundKeyState state,
                FullInputCmdMessage originalMessage)
            {
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
        public override bool HandleCmdMessage(InputCmdMessage message)
        {
            if (!(message is FullInputCmdMessage msg))
                return false;

            switch (msg.State)
            {
                case BoundKeyState.Up:
                    return _disabled?.Invoke(msg.Coordinates, msg.Uid) == true;
                case BoundKeyState.Down:
                    return _enabled?.Invoke(msg.Coordinates, msg.Uid) == true;
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
        public override bool HandleCmdMessage(InputCmdMessage message)
        {
            return true;
        }
    }
}

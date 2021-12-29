using System;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Timing;

namespace OpenNefia.Core.Input
{
    /// <summary>
    ///     Abstract class that all Input Commands derive from.
    /// </summary>
    [Serializable]
    public abstract class InputCmdMessage : EntityEventArgs, IComparable<InputCmdMessage>
    {
        /// <summary>
        ///     The function this command is changing.
        /// </summary>
        public KeyFunctionId InputFunctionId { get; }

        /// <summary>
        /// Sequence number of this input command.
        /// </summary>
        public uint InputSequence { get; set; }

        /// <summary>
        ///     Creates an instance of <see cref="InputCmdMessage"/>.
        /// </summary>
        /// <param name="tick">Client tick this was created.</param>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        public InputCmdMessage(KeyFunctionId inputFunctionId)
        {
            InputFunctionId = inputFunctionId;
        }

        public int CompareTo(InputCmdMessage? other)
        {
            if (other == null)
            {
                return 1;
            }

            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return InputSequence.CompareTo(other.InputSequence);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"seq={InputSequence} func={InputFunctionId}";
        }
    }

    /// <summary>
    ///     An Input Command for a function that has a state.
    /// </summary>
    [Serializable]
    public class StateInputCmdMessage : InputCmdMessage
    {
        /// <summary>
        ///     New state of the Input Function.
        /// </summary>
        public BoundKeyState State { get; }

        /// <summary>
        ///     Creates an instance of <see cref="StateInputCmdMessage"/>.
        /// </summary>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        /// <param name="state">New state of the Input Function.</param>
        public StateInputCmdMessage(KeyFunctionId inputFunctionId, BoundKeyState state)
            : base(inputFunctionId)
        {
            State = state;
        }
    }

    /// <summary>
    ///     A OneShot Input Command that does not have a state.
    /// </summary>
    [Serializable]
    public class EventInputCmdMessage : InputCmdMessage
    {
        /// <summary>
        ///     Creates an instance of <see cref="EventInputCmdMessage"/>.
        /// </summary>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        public EventInputCmdMessage(KeyFunctionId inputFunctionId)
            : base(inputFunctionId) { }
    }

    /// <summary>
    ///     A OneShot Input Command that also contains pointer info.
    /// </summary>
    [Serializable]
    public class PointerInputCmdMessage : EventInputCmdMessage
    {
        /// <summary>
        ///     Local Coordinates of the pointer when the command was created.
        /// </summary>
        public EntityCoordinates Coordinates { get; }

        /// <summary>
        ///     Entity that was under the pointer when the command was created (if any).
        /// </summary>
        public EntityUid Uid { get; }

        /// <summary>
        ///     Creates an instance of <see cref="PointerInputCmdMessage"/>.
        /// </summary>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        /// <param name="coordinates">Local Coordinates of the pointer when the command was created.</param>
        public PointerInputCmdMessage(KeyFunctionId inputFunctionId, EntityCoordinates coordinates)
            : this(inputFunctionId, coordinates, EntityUid.Invalid) { }

        /// <summary>
        ///     Creates an instance of <see cref="PointerInputCmdMessage"/> with an optional Entity reference.
        /// </summary>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        /// <param name="coordinates">Local Coordinates of the pointer when the command was created.</param>
        /// <param name="uid">Entity that was under the pointer when the command was created.</param>
        public PointerInputCmdMessage(KeyFunctionId inputFunctionId, EntityCoordinates coordinates, EntityUid uid)
            : base(inputFunctionId)
        {
            Coordinates = coordinates;
            Uid = uid;
        }
    }

    /// <summary>
    ///     An input command that has both state and pointer info.
    /// </summary>
    [Serializable]
    public class FullInputCmdMessage : InputCmdMessage
    {
        /// <summary>
        ///     New state of the Input Function.
        /// </summary>
        public BoundKeyState State { get; }

        /// <summary>
        ///     Local Coordinates of the pointer when the command was created.
        /// </summary>
        public EntityCoordinates Coordinates { get; }

        /// <summary>
        ///     Screen Coordinates of the pointer when the command was created.
        /// </summary>
        public ScreenCoordinates ScreenCoordinates { get; }

        /// <summary>
        ///     Entity that was under the pointer when the command was created (if any).
        /// </summary>
        public EntityUid Uid { get; }

        /// <summary>
        ///     Creates an instance of <see cref="FullInputCmdMessage"/>.
        /// </summary>
        /// <param name="inputSequence"></param>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        /// <param name="state">New state of the Input Function.</param>
        /// <param name="coordinates">Local Coordinates of the pointer when the command was created.</param>
        /// <param name="screenCoordinates"></param>
        public FullInputCmdMessage(int inputSequence, KeyFunctionId inputFunctionId, BoundKeyState state, EntityCoordinates coordinates, ScreenCoordinates screenCoordinates)
            : this(inputFunctionId, state, coordinates, screenCoordinates, EntityUid.Invalid) { }

        /// <summary>
        ///     Creates an instance of <see cref="FullInputCmdMessage"/> with an optional Entity reference.
        /// </summary>
        /// <param name="inputFunctionId">Function this command is changing.</param>
        /// <param name="state">New state of the Input Function.</param>
        /// <param name="coordinates">Local Coordinates of the pointer when the command was created.</param>
        /// <param name="screenCoordinates"></param>
        /// <param name="uid">Entity that was under the pointer when the command was created.</param>
        public FullInputCmdMessage(KeyFunctionId inputFunctionId, BoundKeyState state, EntityCoordinates coordinates, ScreenCoordinates screenCoordinates, EntityUid uid)
            : base(inputFunctionId)
        {
            State = state;
            Coordinates = coordinates;
            ScreenCoordinates = screenCoordinates;
            Uid = uid;
        }
    }
}

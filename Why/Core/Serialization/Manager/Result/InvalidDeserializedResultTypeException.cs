using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace OpenNefia.Core.Serialization.Manager.Result
{
    public class InvalidDeserializedResultTypeException<TExpected> : Exception
    {
        public readonly Type ReceivedType;

        public override string Message => $"Invalid Type {ReceivedType} received. Expected {typeof(TExpected)}";

        public InvalidDeserializedResultTypeException(Type receivedType)
        {
            ReceivedType = receivedType;
        }

        protected InvalidDeserializedResultTypeException([NotNull] SerializationInfo info, StreamingContext context, Type receivedType) : base(info, context)
        {
            ReceivedType = receivedType;
        }

        public InvalidDeserializedResultTypeException([CanBeNull] string? message, Type receivedType) : base(message)
        {
            ReceivedType = receivedType;
        }

        public InvalidDeserializedResultTypeException([CanBeNull] string? message, [CanBeNull] Exception? innerException, Type receivedType) : base(message, innerException)
        {
            ReceivedType = receivedType;
        }
    }
}

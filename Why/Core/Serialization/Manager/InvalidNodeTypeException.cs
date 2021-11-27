using System;

namespace OpenNefia.Core.Serialization.Manager
{
    public class InvalidNodeTypeException : Exception
    {
        public InvalidNodeTypeException()
        {
        }

        public InvalidNodeTypeException(string? message) : base(message)
        {
        }
    }
}

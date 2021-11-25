using System;

namespace Why.Core.Serialization
{
    public class InvalidMappingException : Exception
    {

        public InvalidMappingException(string msg) : base(msg)
        {

        }
    }
}

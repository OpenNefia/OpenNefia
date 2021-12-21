﻿using System;
using System.Runtime.Serialization;

namespace OpenNefia.Core.Exceptions
{
    [Serializable]
    public class SandboxArgumentException : Exception
    {
        public SandboxArgumentException()
        {
        }

        public SandboxArgumentException(string message) : base(message)
        {
        }

        public SandboxArgumentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SandboxArgumentException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}

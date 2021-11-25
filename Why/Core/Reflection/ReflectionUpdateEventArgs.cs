using System;

namespace Why.Core.Reflection
{
    public class ReflectionUpdateEventArgs : EventArgs
    {
        public readonly IReflectionManager ReflectionManager;
        public ReflectionUpdateEventArgs(IReflectionManager reflectionManager)
        {
            ReflectionManager = reflectionManager;
        }
    }
}

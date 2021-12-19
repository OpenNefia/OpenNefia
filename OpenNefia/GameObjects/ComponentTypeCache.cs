using System;
using System.Threading;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Cache for avoiding typeof overhead when looking up components.
    /// </summary>
    /// <typeparam name="T">Type of the component.</typeparam>
    internal class ComponentTypeCache<T>
    {
        public static readonly int Index;
        public static readonly Type Type;
        static ComponentTypeCache()
        {
            Index = Interlocked.Increment(ref ComponentTypeCounter.TypesCount);
            Type = typeof(T);
        }

        private class ComponentTypeCounter
        {
            public static int TypesCount;
        }
    }
}

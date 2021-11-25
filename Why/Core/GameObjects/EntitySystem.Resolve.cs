using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Why.Core.Log;
using Why.Core.Utility;

namespace Why.Core.GameObjects
{
    public abstract partial class EntitySystem
    {
        /// <summary>
        ///     Resolves the component on the entity but only if the component instance is null.
        /// </summary>
        /// <param name="uid">The entity where to query the components.</param>
        /// <param name="component">A reference to the variable storing the component, or null if it has to be resolved.</param>
        /// <param name="logMissing">Whether to log missing components.</param>
        /// <typeparam name="TComp">The component type to resolve.</typeparam>
        /// <returns>True if the component is not null or was resolved correctly, false if the component couldn't be resolved.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        protected bool Resolve<TComp>(EntityUid uid, [NotNullWhen(true)] ref TComp? component, bool logMissing = true)
            where TComp : IComponent
        {
            DebugTools.Assert(component == null || uid == component.Owner.Uid, "Specified Entity is not the component's Owner!");

            if (component != null)
                return true;

            var found = EntityManager.TryGetComponent(uid, out component);

            if(logMissing && !found)
                Logger.ErrorS("resolve", $"Can't resolve \"{typeof(TComp)}\" on entity {uid}!\n{new StackTrace(1, true)}");

            return found;
        }
    }
}

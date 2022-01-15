using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// A subsystem that acts on all components of a type at once.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public interface IEntitySystem : IEntityEventSubscriber
    {
        /// <summary>
        ///     Called once when the system is created to initialize its state.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Called once before the system is destroyed so that the system can clean up.
        /// </summary>
        void Shutdown();
    }
}

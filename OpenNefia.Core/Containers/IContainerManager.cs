using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Manages containers on an entity.
    /// </summary>
    /// <seealso cref="IContainer" />
    public interface IContainerManager : IComponent
    {
        /// <summary>
        /// DO NOT CALL THIS DIRECTLY. Call <see cref="IContainer.Shutdown" /> instead.
        /// </summary>
        void InternalContainerShutdown(IContainer container);
    }
}

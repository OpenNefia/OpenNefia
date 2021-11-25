using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Why.Core.GameObjects
{
    /// <summary>
    /// Controls <see cref="IEntitySystem"/> instances.
    /// Systems in this iteration act more like bundles of logic 
    /// operating on entities that are wired up to events.
    /// </summary>

    /// <seealso cref="IEntitySystem"/>
    public interface IEntitySystemManager
    {
        /// <summary>
        /// A new entity system has been loaded into the manager.
        /// </summary>
        event EventHandler<SystemChangedArgs> SystemLoaded;

        /// <summary>
        /// An existing entity system has been unloaded from the manager.
        /// </summary>
        event EventHandler<SystemChangedArgs> SystemUnloaded;

        /// <summary>
        /// Get an entity system of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of entity system to find.</typeparam>
        /// <returns>The <see cref="IEntitySystem"/> instance matching the specified type.</returns>
        T GetEntitySystem<T>() where T : IEntitySystem;

        /// <summary>
        /// Resolves an entity system.
        /// </summary>
        /// <exception cref="UnregisteredTypeException">Thrown if the provided type is not registered.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the resolved type hasn't been created yet
        /// because the dependency collection object graph still needs to be constructed for it.
        /// </exception>
        void Resolve<T>([NotNull] ref T? instance)
            where T : IEntitySystem;

        /// <summary>
        /// Tries to get an entity system of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of entity system to find.</typeparam>
        /// <param name="entitySystem">instance matching the specified type (if exists).</param>
        /// <returns>If an instance of the specified entity system type exists.</returns>
        bool TryGetEntitySystem<T>([NotNullWhen(true)] out T? entitySystem) where T : IEntitySystem;

        /// <summary>
        /// Initialize, discover systems and initialize them through <see cref="IEntitySystem.Initialize"/>.
        /// </summary>
        /// <seealso cref="IEntitySystem.Initialize"/>
        void Initialize();

        /// <summary>
        /// Clean up, shut down all systems through <see cref="IEntitySystem.Shutdown"/> and remove them.
        /// </summary>
        /// <seealso cref="IEntitySystem.Shutdown"/>
        void Shutdown();

        void Clear();

        /// <summary>
        ///     Adds an extra entity system type that otherwise would not be loaded automatically, useful for testing.
        /// </summary>
        /// <typeparam name="T">The type of the entity system to load.</typeparam>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the manager has been initialized already.
        /// </exception>
        void LoadExtraSystemType<T>() where T : IEntitySystem, new();
    }
}

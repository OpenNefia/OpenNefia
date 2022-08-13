using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Utility;
using Dependency = OpenNefia.Core.IoC.DependencyAttribute;
#if EXCEPTION_TOLERANCE
using OpenNefia.Core.Exceptions;
#endif


namespace OpenNefia.Core.GameObjects
{
    public class EntitySystemManager : IEntitySystemManager
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IHotReloadWatcher _hotReload = default!;

#if EXCEPTION_TOLERANCE
        [Dependency] private readonly IRuntimeLog _runtimeLog = default!;
#endif

        private DependencyCollection _systemDependencyCollection = default!;
        private List<Type> _systemTypes = new();

        /// <inheritdoc/>
        public IEnumerable<Type> SystemTypes => _systemTypes;

        /// <inheritdoc/>
        public IDependencyCollection DependencyCollection => _systemDependencyCollection;

        private readonly List<Type> _extraLoadedTypes = new();

        private readonly Stopwatch _stopwatch = new();

        private bool _initialized;

        public bool MetricsEnabled { get; set; }

        /// <inheritdoc />
        public event EventHandler<SystemChangedArgs>? SystemLoaded;

        /// <inheritdoc />
        public event EventHandler<SystemChangedArgs>? SystemUnloaded;

        /// <exception cref="UnregisteredTypeException">Thrown if the provided type is not registered.</exception>
        public T GetEntitySystem<T>()
            where T : IEntitySystem
        {
            return _systemDependencyCollection.Resolve<T>();
        }

        public IEntitySystem GetEntitySystem(Type type)
        {
            if (!typeof(IEntitySystem).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type '{type}' does not implement {nameof(IEntitySystem)}");
            }
            
            return (IEntitySystem)_systemDependencyCollection.ResolveType(type);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T>([NotNull] ref T? instance)
            where T : IEntitySystem
        {
            _systemDependencyCollection.Resolve(ref instance);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2)
            where T1 : IEntitySystem
            where T2 : IEntitySystem
        {
            _systemDependencyCollection.Resolve(ref instance1, ref instance2);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2, T3>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2, [NotNull] ref T3? instance3)
            where T1 : IEntitySystem
            where T2 : IEntitySystem
            where T3 : IEntitySystem
        {
            _systemDependencyCollection.Resolve(ref instance1, ref instance2, ref instance3);
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        public void Resolve<T1, T2, T3, T4>([NotNull] ref T1? instance1, [NotNull] ref T2? instance2, [NotNull] ref T3? instance3, [NotNull] ref T4? instance4)
            where T1 : IEntitySystem
            where T2 : IEntitySystem
            where T3 : IEntitySystem
            where T4 : IEntitySystem
        {
            _systemDependencyCollection.Resolve(ref instance1, ref instance2, ref instance3, ref instance4);
        }

        /// <inheritdoc />
        public bool TryGetEntitySystem<T>([NotNullWhen(true)] out T? entitySystem)
            where T : IEntitySystem
        {
            return _systemDependencyCollection.TryResolveType<T>(out entitySystem);
        }

        /// <inheritdoc />
        public bool TryGetEntitySystem(Type type, [NotNullWhen(true)] out IEntitySystem? entitySystem)
        {
            if (!typeof(IEntitySystem).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type '{type}' does not implement {nameof(IEntitySystem)}");
            }

            var result = _systemDependencyCollection.TryResolveType(type, out var obj);
            entitySystem = obj as IEntitySystem;
            return result;
        }

        /// <inheritdoc />
        public void Initialize()
        {
            var excludedTypes = new HashSet<Type>();

            _systemDependencyCollection = new(IoCManager.Instance!);
            var subTypes = new Dictionary<Type, Type>();
            _systemTypes.Clear();
            foreach (var type in _reflectionManager.GetAllChildren<IEntitySystem>().Concat(_extraLoadedTypes))
            {
                Logger.DebugS("go.sys", "Initializing entity system {0}", type);

                _systemDependencyCollection.Register(type);
                _systemTypes.Add(type);

                excludedTypes.Add(type);
                if (subTypes.ContainsKey(type))
                {
                    subTypes.Remove(type);
                }

                // also register systems under their supertypes, so they can be retrieved by their supertype.
                // We don't do this if there are multiple subtype systems of that supertype though, otherwise
                // it wouldn't be clear which instance to return when asking for the supertype
                foreach (var baseType in GetBaseTypes(type))
                {
                    // already known that there are multiple subtype systems of this type,
                    // so don't register under the supertype because it would be unclear
                    // which instance to return if we retrieved it by the supertype
                    if (excludedTypes.Contains(baseType)) continue;

                    if (subTypes.ContainsKey(baseType))
                    {
                        subTypes.Remove(baseType);
                        excludedTypes.Add(baseType);
                    }
                    else
                    {
                        subTypes.Add(baseType, type);
                    }
                }
            }

            foreach (var (baseType, type) in subTypes)
            {
                _systemDependencyCollection.Register(baseType, type, overwrite: true);
                _systemTypes.Remove(baseType);
            }

            _systemDependencyCollection.BuildGraph();
            _systemTypes.Sort((a, b) => a.FullName!.CompareTo(b.FullName));

            foreach (var systemType in _systemTypes)
            {
                var system = (IEntitySystem)_systemDependencyCollection.ResolveType(systemType);
                system.Initialize();
                SystemLoaded?.Invoke(this, new SystemChangedArgs(system));
            }

            _hotReload.OnUpdateApplication += OnUpdateApplication;

            Logger.InfoS("esm", $"Registered {_systemTypes.Count} entity systems.");
            _initialized = true;
        }

        private void OnUpdateApplication(HotReloadEventArgs args)
        {
            if (args.UpdatedTypes == null)
                return;

            foreach (var type in args.UpdatedTypes)
            {
                if (typeof(IEntitySystem).IsAssignableFrom(type))
                {
                    if (TryGetEntitySystem(type, out var system))
                    {
                        Logger.InfoS("esm", $"Reinjecting dependencies for hotloaded system {type}.");
                        EntitySystem.InjectDependencies(system);
                    }
                }
                else if (IoCManager.Instance?.TryResolveType(type, out var instance) ?? false)
                {
                    Logger.InfoS("esm", $"Reinjecting dependencies for hotloaded IoC type {type}.");
                    IoCManager.InjectDependencies(instance);
                }
            }
        }

        private static IEnumerable<Type> GetBaseTypes(Type type) {
            if(type.BaseType == null) return type.GetInterfaces();

            return Enumerable.Repeat(type.BaseType, 1)
                .Concat(type.GetInterfaces())
                .Concat(type.GetInterfaces().SelectMany<Type, Type>(GetBaseTypes))
                .Concat(GetBaseTypes(type.BaseType));
        }

        /// <inheritdoc />
        public void Shutdown()
        {
            // System.Values is modified by RemoveSystem
            foreach (var systemType in _systemTypes)
            {
                if(_systemDependencyCollection == null) continue;
                var system = (IEntitySystem)_systemDependencyCollection.ResolveType(systemType);
                SystemUnloaded?.Invoke(this, new SystemChangedArgs(system));
                system.Shutdown();
                _entityManager.EventBus.UnsubscribeEvents(system);
            }

            Clear();
        }

        public void Clear()
        {
            _systemTypes.Clear();
            _initialized = false;
            _systemDependencyCollection?.Clear();
        }

        public void LoadExtraSystemType<T>() where T : IEntitySystem, new()
            => LoadExtraSystemType(typeof(T));

        public void LoadExtraSystemType(Type type)
        {
            if (_initialized)
            {
                throw new InvalidOperationException(
                    "Cannot use LoadExtraSystemType when the entity system manager is initialized.");
            }

            if (!typeof(IEntitySystem).IsAssignableFrom(type))
            {
                throw new InvalidOperationException(
                    $"Type {type} does not implement {nameof(IEntitySystem)}.");
            }

            _extraLoadedTypes.Add(type);
        }

        /// <inheritdoc />
        public T InjectDependencies<T>(T obj) where T : notnull
        {
            if (_systemDependencyCollection == null)
            {
                throw new InvalidOperationException(
                    "Cannot use InjectDependencies when the entity system manager is not initialized yet.");
            }
            _systemDependencyCollection.InjectDependencies(obj);
            return obj;
        }

        private static bool NeedsUpdate(Type type)
        {
            if (!typeof(EntitySystem).IsAssignableFrom(type))
            {
                return true;
            }

            var mUpdate = type.GetMethod(nameof(EntitySystem.Update), new[] {typeof(float)});

            DebugTools.AssertNotNull(mUpdate);

            return mUpdate!.DeclaringType != typeof(EntitySystem);
        }

        private static bool NeedsFrameUpdate(Type type)
        {
            if (!typeof(EntitySystem).IsAssignableFrom(type))
            {
                return true;
            }

            var mFrameUpdate = type.GetMethod(nameof(EntitySystem.FrameUpdate), new[] {typeof(float)});

            DebugTools.AssertNotNull(mFrameUpdate);

            return mFrameUpdate!.DeclaringType != typeof(EntitySystem);
        }
    }

    public class SystemChangedArgs : EventArgs
    {
        public IEntitySystem System { get; }

        public SystemChangedArgs(IEntitySystem system)
        {
            System = system;
        }
    }
}

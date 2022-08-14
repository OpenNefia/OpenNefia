using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
#if EXCEPTION_TOLERANCE
using OpenNefia.Core.Exceptions;
#endif
using DependencyAttribute = OpenNefia.Core.IoC.DependencyAttribute;

namespace OpenNefia.Core.GameObjects
{
    /// <inheritdoc />
    public partial class EntityManager
    {
        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly IComponentDependencyManager _componentDependencyManager = default!;

#if EXCEPTION_TOLERANCE
        [Dependency] private readonly IRuntimeLog _runtimeLog = default!;
#endif

        public IComponentFactory ComponentFactory => _componentFactory;

        private const int TypeCapacity = 32;
        private const int ComponentCollectionCapacity = 1024;
        private const int EntityCapacity = 1024;
        private const int NetComponentCapacity = 8;

        private readonly Dictionary<Type, Dictionary<EntityUid, Component>> _entTraitDict
            = new();

        private Dictionary<EntityUid, Component>[] _entTraitArray
            = Array.Empty<Dictionary<EntityUid, Component>>();

        private readonly HashSet<Component> _deleteSet = new(TypeCapacity);

        private UniqueIndexHkm<EntityUid, Component> _entCompIndex =
            new(ComponentCollectionCapacity);

        /// <inheritdoc />
        public event EventHandler<ComponentEventArgs>? ComponentAdded;

        /// <inheritdoc />
        public event EventHandler<ComponentEventArgs>? ComponentRemoved;

        /// <inheritdoc />
        public event EventHandler<ComponentEventArgs>? ComponentDeleted;

        public void InitializeComponents()
        {
            if (Initialized)
                throw new InvalidOperationException("Already initialized.");

            FillComponentDict();
            _componentFactory.ComponentAdded += OnComponentAdded;
            _componentFactory.ComponentReferenceAdded += OnComponentReferenceAdded;
        }

        /// <summary>
        ///     Instantly clears all components from the manager. This will NOT shut them down gracefully.
        ///     Any entities relying on existing components will be broken.
        /// </summary>
        public void ClearComponents()
        {
            _componentFactory.ComponentAdded -= OnComponentAdded;
            _componentFactory.ComponentReferenceAdded -= OnComponentReferenceAdded;
            _entCompIndex.Clear();
            _deleteSet.Clear();
            FillComponentDict();
        }

        private void AddComponentRefType(Type type)
        {
            var dict = new Dictionary<EntityUid, Component>();
            _entTraitDict.Add(type, dict);
            var index = GetCompIdIndex(type);
            EnsureEntTraitIndexCapacity(index);
            _entTraitArray[index] = dict;
        }

        private void OnComponentAdded(IComponentRegistration obj)
        {
            AddComponentRefType(obj.Type);
        }

        private void OnComponentReferenceAdded((IComponentRegistration, Type type) obj)
        {
            AddComponentRefType(obj.Item2);
        }

        private static int GetCompIdIndex(Type type)
        {
            return (int)typeof(CompArrayIndex<>)
                .MakeGenericType(type)
                .GetField(nameof(CompArrayIndex<int>.Index), BindingFlags.Static | BindingFlags.Public)!
                .GetValue(null)!;
        }

        private void EnsureEntTraitIndexCapacity(int index)
        {
            var curLength = _entTraitArray.Length;
            if (curLength > index)
                return;

            // The array indices are static across all EntityManager instances so they
            // won't get reset between test runs. So it's possible for a test to suddenly
            // request a component type with large index like 88 without any array capacity
            // being allocated yet. That's why "index" has to be used instead of "curLength".
            var newLength = MathHelper.NextPowerOfTwo(Math.Max(8, index));
            Array.Resize(ref _entTraitArray, newLength);
        }

        #region Component Management

        public void InitializeComponents(EntityUid uid)
        {
            var metadata = GetComponent<MetaDataComponent>(uid);
            DebugTools.Assert(metadata.EntityLifeStage == EntityLifeStage.PreInit);
            metadata.EntityLifeStage = EntityLifeStage.Initializing;

            // Initialize() can modify the collection of components.
            var components = GetComponents(uid)
                         .OrderBy(x => x switch
                          {
                              SpatialComponent _ => 0,
                              _ => int.MaxValue
                          });

            foreach (var component in components)
            {
                var comp = (Component)component;
                if (comp.Initialized)
                    continue;

                comp.LifeInitialize();
            }

#if DEBUG
            // Second integrity check in case of.
            foreach (var t in GetComponents(uid))
            {
                if (!t.Initialized)
                {
                    DebugTools.Assert($"Component {t.Name} was not initialized at the end of {nameof(InitializeComponents)}.");
                }
            }

#endif
            DebugTools.Assert(metadata.EntityLifeStage == EntityLifeStage.Initializing);
            metadata.EntityLifeStage = EntityLifeStage.Initialized;
            EventBus.RaiseEvent(new EntityInitializedEvent(uid));
        }

        public void StartComponents(EntityUid uid)
        {
            // TODO: Move this to EntityManager.
            // Startup() can modify _components
            // This code can only handle additions to the list. Is there a better way? Probably not.
            var comps = GetComponents(uid)
                     .OrderBy(x => x switch
                       {
                           SpatialComponent _ => 0,
                           _ => int.MaxValue
                       });

            foreach (var component in comps)
            {
                var comp = (Component)component;
                if (comp.LifeStage == ComponentLifeStage.Initialized)
                {
                    comp.LifeStartup();
                }
            }
        }

        /// <inheritdoc/>
        public IComponent AddComponent(EntityUid uid, Type type)
        {
            if (!uid.IsValid() || !EntityExists(uid))
                throw new ArgumentException("Entity is not valid or deleted.", nameof(uid));

            var newComponent = (Component)_componentFactory.GetComponent(type);

            newComponent.Owner = uid;

            AddComponent(uid, newComponent);

            return newComponent;
        }

        /// <inheritdoc/>
        public T AddComponent<T>(EntityUid uid) where T : Component, new()
        {
            if (!uid.IsValid() || !EntityExists(uid))
                throw new ArgumentException("Entity is not valid or deleted.", nameof(uid));

            var newComponent = _componentFactory.GetComponent<T>();

            newComponent.Owner = uid;

            AddComponent(uid, newComponent);

            return newComponent;
        }

        /// <inheritdoc/>
        public void AddComponent<T>(EntityUid uid, T component, bool overwrite = false) where T : Component
        {
            if (!uid.IsValid() || !EntityExists(uid))
                throw new ArgumentException("Entity is not valid.", nameof(uid));

            if (component == null) throw new ArgumentNullException(nameof(component));

            if (component.Owner != uid) throw new InvalidOperationException("Component is not owned by entity.");

            AddComponentInternal(uid, component, overwrite);
        }

        private void AddComponentInternal<T>(EntityUid uid, T component, bool overwrite = false) where T : Component
        {
            // get interface aliases for mapping
            var reg = _componentFactory.GetRegistration(component);

            // Check that there are no overlapping references.
            foreach (var type in reg.References)
            {
                var dict = _entTraitDict[type];
                if (!dict.TryGetValue(uid, out var duplicate))
                    continue;

                if (!overwrite && !duplicate.Deleted)
                    throw new InvalidOperationException(
                        $"Component reference type {type} already occupied by {duplicate}");

                // metadata are required on all entities and cannot be overwritten.
                if (duplicate is MetaDataComponent)
                    throw new InvalidOperationException("Tried to overwrite a protected component.");

                RemoveComponentImmediate(duplicate, uid, false);
            }

            // add the component to the grid
            foreach (var type in reg.References)
            {
                _entTraitDict[type].Add(uid, component);
                _entCompIndex.Add(uid, component);
            }

            ComponentAdded?.Invoke(this, new AddedComponentEventArgs(component, uid));

            _componentDependencyManager.OnComponentAdd(uid, component);

            component.LifeAddToEntity();

            var metadata = GetComponent<MetaDataComponent>(uid);

            if (!metadata.EntityInitialized && !metadata.EntityInitializing)
                return;

            component.LifeInitialize();

            if (metadata.EntityInitialized)
                component.LifeStartup();
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent<T>(EntityUid uid)
        {
            RemoveComponent(uid, typeof(T));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(EntityUid uid, Type type)
        {
            RemoveComponentImmediate((Component)GetComponent(uid, type), uid, false);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveComponent(EntityUid uid, IComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (component.Owner != uid)
                throw new InvalidOperationException("Component is not owned by entity.");

            RemoveComponentImmediate((Component)component, uid, false);
        }

        private static IEnumerable<Component> InSafeOrder(IEnumerable<Component> comps, bool forCreation = false)
        {
            static int Sequence(IComponent x)
                => x switch
                {
                    MetaDataComponent => 0,
                    _ => int.MaxValue
                };

            return forCreation
                ? comps.OrderBy(Sequence)
                : comps.OrderByDescending(Sequence);
        }

        /// <inheritdoc />
        public void RemoveComponents(EntityUid uid)
        {
            foreach (var comp in InSafeOrder(_entCompIndex[uid]))
            {
                RemoveComponentImmediate(comp, uid, false);
            }
        }

        /// <inheritdoc />
        public void DisposeComponents(EntityUid uid)
        {
            foreach (var comp in InSafeOrder(_entCompIndex[uid]))
            {
                RemoveComponentImmediate(comp, uid, true);
            }

            // DisposeComponents means the entity is getting deleted.
            // Safe to wipe the entity out of the index.
            _entCompIndex.Remove(uid);
        }

        private void RemoveComponentDeferred(Component component, EntityUid uid, bool removeProtected)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            if (component.Deleted) return;

#if EXCEPTION_TOLERANCE
            try
            {
#endif
            // metadata are required on all entities and cannot be removed normally.
            if (!removeProtected && component is MetaDataComponent)
            {
                DebugTools.Assert("Tried to remove a protected component.");
                return;
            }

            if (!_deleteSet.Add(component))
            {
                // already deferred deletion
                return;
            }

            if (component.Running)
                component.LifeShutdown();

            if (component.LifeStage != ComponentLifeStage.PreAdd)
                component.LifeRemoveFromEntity();
            _componentDependencyManager.OnComponentRemove(uid, component);
            ComponentRemoved?.Invoke(this, new RemovedComponentEventArgs(component, uid));
#if EXCEPTION_TOLERANCE
            }
            catch (Exception e)
            {
                _runtimeLog.LogException(e,
                    $"RemoveComponentDeferred, owner={component.Owner}, type={component.GetType()}");
            }
#endif
        }

        private void RemoveComponentImmediate(Component component, EntityUid uid, bool removeProtected)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

#if EXCEPTION_TOLERANCE
            try
            {
#endif
            if (!component.Deleted)
            {
                // metadata are required on all entities and cannot be removed.
                if (!removeProtected && component is MetaDataComponent)
                {
                    DebugTools.Assert("Tried to remove a protected component.");
                    return;
                }

                if (component.Running)
                    component.LifeShutdown();

                if (component.LifeStage != ComponentLifeStage.PreAdd)
                    component.LifeRemoveFromEntity(); // Sets delete

                _componentDependencyManager.OnComponentRemove(uid, component);
                ComponentRemoved?.Invoke(this, new RemovedComponentEventArgs(component, uid));
            }
#if EXCEPTION_TOLERANCE
            }
            catch (Exception e)
            {
                _runtimeLog.LogException(e,
                    $"RemoveComponentImmediate, owner={component.Owner}, type={component.GetType()}");
            }
#endif

            DeleteComponent(component);
        }

        /// <inheritdoc />
        public void CullRemovedComponents()
        {
            foreach (var component in InSafeOrder(_deleteSet))
            {
                DeleteComponent(component);
            }

            _deleteSet.Clear();
        }

        private void DeleteComponent(Component component)
        {
            var reg = _componentFactory.GetRegistration(component.GetType());

            var entityUid = component.Owner;

            foreach (var refType in reg.References)
            {
                _entTraitDict[refType].Remove(entityUid);
            }

            _entCompIndex.Remove(entityUid, component);
            ComponentDeleted?.Invoke(this, new DeletedComponentEventArgs(component, entityUid));
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>(EntityUid uid)
        {
            return _entTraitArray[ArrayIndexFor<T>()].TryGetValue(uid, out var comp) && !comp.Deleted;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent(EntityUid uid, Type type)
        {
            var dict = _entTraitDict[type];
            return dict.TryGetValue(uid, out var comp) && !comp.Deleted;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasComponent<T>(EntityUid? uid)
        {
            return uid.HasValue && HasComponent<T>(uid.Value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T EnsureComponent<T>(EntityUid uid) where T : Component, new()
        {
            if (TryGetComponent<T>(uid, out var component))
                return component;

            return AddComponent<T>(uid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IComponent EnsureComponent(EntityUid uid, Type type)
        {
            if (TryGetComponent(uid, type, out var component))
                return component;

            return AddComponent(uid, type);
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetComponent<T>(EntityUid uid)
        {
            var dict = _entTraitArray[ArrayIndexFor<T>()];
            if (dict.TryGetValue(uid, out var comp))
            {
                if (!comp.Deleted)
                {
                    return (T)(IComponent)comp;
                }
            }

            throw new KeyNotFoundException($"Entity {uid} does not have a component of type {typeof(T)}");
        }

        /// <inheritdoc />
        public IComponent GetComponent(EntityUid uid, Type type)
        {
            // ReSharper disable once InvertIf
            var dict = _entTraitDict[type];
            if (dict.TryGetValue(uid, out var comp))
            {
                if (!comp.Deleted)
                {
                    return comp;
                }
            }

            throw new KeyNotFoundException($"Entity {uid} does not have a component of type {type}");
        }

        /// <inheritdoc />
        public bool TryGetComponent<T>(EntityUid uid, [NotNullWhen(true)] out T component)
        {
            var dict = _entTraitArray[ArrayIndexFor<T>()];
            if (dict.TryGetValue(uid, out var comp))
            {
                if (!comp.Deleted)
                {
                    component = (T)(IComponent)comp;
                    return true;
                }
            }

            component = default!;
            return false;
        }

        /// <inheritdoc />
        public bool TryGetComponent(EntityUid uid, Type type, [NotNullWhen(true)] out IComponent? component)
        {
            var dict = _entTraitDict[type];
            if (dict.TryGetValue(uid, out var comp))
            {
                if (!comp.Deleted)
                {
                    component = comp;
                    return true;
                }
            }

            component = null;
            return false;
        }

        /// <inheritdoc />
        public IEnumerable<IComponent> GetComponents(EntityUid uid)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (Component comp in _entCompIndex[uid].ToArray())
            {
                if (comp.Deleted) continue;

                yield return comp;
            }
        }

        /// <inheritdoc />
        public IEnumerable<T> GetComponents<T>(EntityUid uid)
        {
            var comps = _entCompIndex[uid].ToArray();
            foreach (var comp in comps)
            {
                if (comp.Deleted || comp is not T tComp) continue;

                yield return tComp;
            }
        }

        #region Join Functions

        /// <inheritdoc />
        public IEnumerable<T> EntityQuery<T>() where T : IComponent
        {
            var comps = _entTraitArray[ArrayIndexFor<T>()];

            foreach (var t1Comp in comps.Values)
            {
                if (t1Comp.Deleted) continue;

                yield return (T)(object)t1Comp;
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2)> EntityQuery<TComp1, TComp2>()
            where TComp1 : IComponent
            where TComp2 : IComponent
        {
            // this would prob be faster if trait1 was a list (or an array of structs hue).
            var trait1 = _entTraitArray[ArrayIndexFor<TComp1>()];
            var trait2 = _entTraitArray[ArrayIndexFor<TComp2>()];

            // you really want trait1 to be the smaller set of components
            foreach (var (uid, t1Comp) in trait1)
            {
                if (!trait2.TryGetValue(uid, out var t2Comp) || t2Comp.Deleted)
                    continue;

                yield return (
                    (TComp1)(object)t1Comp,
                    (TComp2)(object)t2Comp);
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2, TComp3)> EntityQuery<TComp1, TComp2, TComp3>()
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
        {
            var trait1 = _entTraitArray[ArrayIndexFor<TComp1>()];
            var trait2 = _entTraitArray[ArrayIndexFor<TComp2>()];
            var trait3 = _entTraitArray[ArrayIndexFor<TComp3>()];

            foreach (var (uid, t1Comp) in trait1)
            {
                if (!trait2.TryGetValue(uid, out var t2Comp) || t2Comp.Deleted)
                    continue;

                if (!trait3.TryGetValue(uid, out var t3Comp) || t3Comp.Deleted)
                    continue;

                yield return (
                    (TComp1)(object)t1Comp,
                    (TComp2)(object)t2Comp,
                    (TComp3)(object)t3Comp);
            }
        }

        /// <inheritdoc />
        public IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQuery<TComp1, TComp2, TComp3, TComp4>()
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent
        {
            var trait1 = _entTraitArray[ArrayIndexFor<TComp1>()];
            var trait2 = _entTraitArray[ArrayIndexFor<TComp2>()];
            var trait3 = _entTraitArray[ArrayIndexFor<TComp3>()];
            var trait4 = _entTraitArray[ArrayIndexFor<TComp4>()];

            foreach (var (uid, t1Comp) in trait1)
            {
                if (!trait2.TryGetValue(uid, out var t2Comp) || t2Comp.Deleted)
                    continue;

                if (!trait3.TryGetValue(uid, out var t3Comp) || t3Comp.Deleted)
                    continue;

                if (!trait4.TryGetValue(uid, out var t4Comp) || t4Comp.Deleted)
                    continue;

                yield return (
                    (TComp1)(object)t1Comp,
                    (TComp2)(object)t2Comp,
                    (TComp3)(object)t3Comp,
                    (TComp4)(object)t4Comp);
            }
        }

        #endregion

        /// <inheritdoc />
        public IEnumerable<IComponent> GetAllComponents(Type type)
        {
            var comps = _entTraitDict[type];
            foreach (var comp in comps.Values)
            {
                if (comp.Deleted) continue;

                yield return comp;
            }
        }

        /// <inheritdoc />
        public IEnumerable<T> GetAllComponents<T>()
            where T : IComponent
        {
            return GetAllComponents(typeof(T)).Cast<T>();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void FillComponentDict()
        {
            _entTraitDict.Clear();
            Array.Fill(_entTraitArray, null);

            foreach (var refType in _componentFactory.GetAllRefTypes())
            {
                AddComponentRefType(refType);
            }
        }

        private static int ArrayIndexFor<T>() => CompArrayIndex<T>.Index;

        private static int _compIndexMaster = -1;

        private static class CompArrayIndex<T>
        {
            // ReSharper disable once StaticMemberInGenericType
            public static readonly int Index = Interlocked.Increment(ref _compIndexMaster);
        }
    }
}

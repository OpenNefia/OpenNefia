using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using Spectre.Console;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Prototypes
{
    /// <summary>
    /// Handle storage and loading of YAML prototypes.
    /// </summary>
    public interface IPrototypeManager
    {
        void Initialize();

        /// <summary>
        /// Return an IEnumerable to iterate all prototypes of a certain type.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        IEnumerable<T> EnumeratePrototypes<T>() where T : class, IPrototype;

        /// <summary>
        /// Return an IEnumerable to iterate all prototypes of a certain type.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        IEnumerable<IPrototype> EnumeratePrototypes(Type type);

        /// <summary>
        /// Return an IEnumerable to iterate all prototypes of a certain variant.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the variant of prototype is not registered.
        /// </exception>
        IEnumerable<IPrototype> EnumeratePrototypes(string variant);

        /// <summary>
        /// Returns an IEnumerable to iterate all parents of a prototype of a certain type.
        /// </summary>
        IEnumerable<T> EnumerateParents<T>(PrototypeId<T> id, bool includeSelf = false) where T : class, IPrototype, IInheritingPrototype;

        /// <summary>
        /// Returns an IEnumerable to iterate all parents of a prototype of a certain type.
        /// </summary>
        IEnumerable<IPrototype> EnumerateParents(Type type, string id, bool includeSelf = false);

        /// <summary>
        /// Index for a <see cref="IPrototype"/> by ID.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        T Index<T>(PrototypeId<T> id) where T : class, IPrototype;

        /// <summary>
        /// Index for a <see cref="IPrototype"/> by ID.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the ID does not exist or the type of prototype is not registered.
        /// </exception>
        IPrototype Index(Type type, string id);

        /// <summary>
        ///     Returns whether a prototype of type <typeparamref name="T"/> with the specified <param name="id"/> exists.
        /// </summary>
        bool HasIndex<T>(PrototypeId<T> id) where T : class, IPrototype;
        bool HasIndex(Type type, string id);
        bool TryIndex<T>(PrototypeId<T> id, [NotNullWhen(true)] out T? prototype, bool logMissing = true) where T : class, IPrototype;
        bool TryIndex(Type type, string id, [NotNullWhen(true)] out IPrototype? prototype, bool logMissing = true);

        bool HasMapping<T>(PrototypeId<T> id) where T : class, IPrototype;
        bool TryGetMapping(Type type, string id, [NotNullWhen(true)] out MappingDataNode? mappings);

        /// <summary>
        /// Index for a <see cref="IPrototype"/>'s extended data.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        TExt GetExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;

        /// <summary>
        /// Index for a <see cref="IPrototype"/>'s extended data.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        TExt GetExtendedData<TProto, TExt>(TProto proto)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;

        /// <summary>
        /// Index for a <see cref="IPrototype"/>'s extended data.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the ID does not exist or the type of prototype is not registered.
        /// </exception>
        IPrototypeExtendedData GetExtendedData(Type protoType, Type extType, string id);

        bool HasExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;
        bool HasExtendedData<TProto, TExt>(TProto proto)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;
        bool HasExtendedData(Type protoType, Type extType, string id);
        bool TryGetExtendedData<TProto, TExt>(PrototypeId<TProto> id, [NotNullWhen(true)] out TExt? data)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;
        bool TryGetExtendedData<TProto, TExt>(TProto proto, [NotNullWhen(true)] out TExt? data)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>;
        bool TryGetExtendedData(Type protoType, Type extType, string id, [NotNullWhen(true)] out IPrototypeExtendedData? data);

        IPrototypeComparer<T> GetComparator<T>() where T : class, IPrototype;

        /// <summary>
        ///     Returns whether a prototype variant <param name="variant"/> exists.
        /// </summary>
        /// <param name="variant">Identifier for the prototype variant.</param>
        /// <returns>Whether the prototype variant exists.</returns>
        bool HasVariant(string variant);

        /// <summary>
        ///     Returns the Type for a prototype variant.
        /// </summary>
        /// <param name="variant">Identifier for the prototype variant.</param>
        /// <returns>The specified prototype Type.</returns>
        /// <exception cref="KeyNotFoundException">
        ///     Thrown when the specified prototype variant isn't registered or doesn't exist.
        /// </exception>
        Type GetVariantType(string variant);

        /// <summary>
        ///     Attempts to get the Type for a prototype variant.
        /// </summary>
        /// <param name="variant">Identifier for the prototype variant.</param>
        /// <param name="prototype">The specified prototype Type, or null.</param>
        /// <returns>Whether the prototype type was found and <see cref="prototype"/> isn't null.</returns>
        bool TryGetVariantType(string variant, [NotNullWhen(true)] out Type? prototype);

        /// <summary>
        ///     Attempts to get a prototype's variant.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="variant"></param>
        /// <returns></returns>
        bool TryGetVariantFrom(Type type, [NotNullWhen(true)] out string? variant);

        /// <summary>
        ///     Attempts to get a prototype's variant.
        /// </summary>
        /// <param name="prototype">The prototype in question.</param>
        /// <param name="variant">Identifier for the prototype variant, or null.</param>
        /// <returns>Whether the prototype variant was successfully retrieved.</returns>
        bool TryGetVariantFrom(IPrototype prototype, [NotNullWhen(true)] out string? variant);

        /// <summary>
        ///     Attempts to get a prototype's variant.
        /// </summary>
        /// <param name="variant">Identifier for the prototype variant, or null.</param>
        /// <typeparam name="T">The prototype in question.</typeparam>
        /// <returns>Whether the prototype variant was successfully retrieved.</returns>
        bool TryGetVariantFrom<T>([NotNullWhen(true)] out string? variant) where T : class, IPrototype;

        /// <summary>
        /// Load prototypes from files in a directory, recursively.
        /// </summary>
        void LoadDirectory(ResourcePath path, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null);

        Dictionary<string, HashSet<ErrorNode>> ValidateDirectory(ResourcePath path);

        void LoadFromStream(TextReader stream, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null);

        void LoadString(string str, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null);

        void RemoveString(string prototypes);

        /// <summary>
        /// Clear out all prototypes and reset to a blank slate.
        /// </summary>
        void Clear();

        /// <summary>
        /// Syncs all inter-prototype data. Call this when operations adding new prototypes are done.
        /// </summary>
        void ResolveResults();

        void RegisterEvents();

        /// <summary>
        /// Loads a single prototype class type into the manager.
        /// </summary>
        /// <typeparam name="T">A prototype class type that implements IPrototype. This type also
        /// requires a <see cref="PrototypeAttribute"/> with a non-empty class string.</typeparam>
        void RegisterType<T>() where T : IPrototype;

        /// <summary>
        /// Loads a single prototype class type into the manager.
        /// </summary>
        /// <param name="protoClass">A prototype class type that implements IPrototype. This type also
        /// requires a <see cref="PrototypeAttribute"/> with a non-empty class string.</param>
        void RegisterType(Type protoClass);

        IPrototypeEventBus EventBus { get; }

        event Action<YamlStream, string>? LoadedData;

        /// <summary>
        ///     Fired when prototype are reloaded. The event args contain the modified prototypes.
        /// </summary>
        /// <remarks>
        ///     This *also* fires on initial prototype load.
        /// </remarks>
        event Action<PrototypesReloadedEventArgs> PrototypesReloaded;

        /// <summary>
        ///     Fired before each prototype node is loaded, to allow transforming it.
        /// </summary>
        event BeforePrototypeLoadDelegate BeforePrototypeLoad;
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IPrototypeExtendedData
    {
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IPrototypeExtendedData<T> : IPrototypeExtendedData
        where T : IPrototype
    {
    }

    /// <summary>
    /// Quick attribute to give the prototype its type string.
    /// To prevent needing to instantiate it because interfaces can't declare statics.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    [BaseTypeRequired(typeof(IPrototype))]
    [MeansImplicitUse]
    [MeansDataDefinition]
    public class PrototypeAttribute : Attribute
    {
        private readonly string type;
        public string Type => type;
        public readonly int LoadPriority = 1;

        public PrototypeAttribute(string type, int loadPriority = 1)
        {
            this.type = type;
            LoadPriority = loadPriority;
        }
    }

    internal interface IPrototypeManagerInternal : IPrototypeManager
    {
        IEnumerable<Type> EnumeratePrototypeTypes();
        IEnumerable<PrototypeInheritance> EnumerateInheritanceTree(Type type);
    }

    public delegate void BeforePrototypeLoadDelegate(Type prototypeType, string prototypeTypeName, string prototypeID, MappingDataNode yaml);

    public sealed record class PrototypeInheritance(Type Type, string ID, string Parent);

    public sealed record PrototypeOrderingData(string[] Before, string[] After);

    public sealed partial class PrototypeManager : IPrototypeManagerInternal
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IResourceManager Resources = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IEntityFactory _entityFactory = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

        private readonly Dictionary<string, Type> _prototypeTypes = new();
        private readonly Dictionary<Type, int> _prototypePriorities = new();

        private PrototypeEventBus _eventBus = null!;

        /// <inheritdoc />
        public IPrototypeEventBus EventBus => _eventBus;

        private bool _initialized;
        private bool _hasEverBeenReloaded;

        #region IPrototypeManager members

        private readonly Dictionary<Type, Dictionary<string, IPrototype>> _prototypes = new();
        private readonly Dictionary<Type, Dictionary<string, MappingDataNode>> _prototypeResults = new();
        private readonly Dictionary<Type, MultiRootInheritanceGraph<string>> _inheritanceTrees = new();
        private readonly Dictionary<Type, Dictionary<string, PrototypeOrderingData>> _prototypeOrdering = new();
        private readonly Dictionary<Type, List<IPrototype>> _sortedPrototypes = new();
        private readonly Dictionary<Type, Dictionary<string, int>> _sortedPrototypeIndices = new();
        private readonly Dictionary<Type, Dictionary<string, Dictionary<Type, IPrototypeExtendedData>>> _prototypeExtendedData = new();
        private readonly Dictionary<Type, Dictionary<string, List<PrototypeEventHandlerDef>>> _prototypeEventDefs = new();
        private readonly Dictionary<Type, Dictionary<string, Exception>> _prototypeErrors = new();

        private static Dictionary<T1, Dictionary<T2, T3>> Deepcopy<T1, T2, T3>(Dictionary<T1, Dictionary<T2, T3>> dict)
            where T1 : class
            where T2 : class
        {
            // Deepcopy two layers worth of dictionaries.
            var result = new Dictionary<T1, Dictionary<T2, T3>>();
            foreach (var (a, b) in dict)
            {
                var inner = new Dictionary<T2, T3>();
                foreach (var (prototypeId, res) in b)
                {
                    inner.Add(prototypeId, res);
                }
                result.Add(a, inner);
            }
            return result;
        }

        public void Initialize()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"{nameof(PrototypeManager)} has already been initialized.");
            }

            _initialized = true;
            ReloadPrototypeTypes();

            _eventBus = new PrototypeEventBus(this);

            _graphics.OnWindowFocusChanged += WindowFocusedChanged;

            WatchResources();
        }

        public IEnumerable<T> EnumeratePrototypes<T>() where T : class, IPrototype
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            return _sortedPrototypes[typeof(T)].Select(p => (T)p);
        }

        public IEnumerable<IPrototype> EnumeratePrototypes(Type type)
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            return _sortedPrototypes[type];
        }

        public IEnumerable<IPrototype> EnumeratePrototypes(string variant)
        {
            return EnumeratePrototypes(GetVariantType(variant));
        }

        public IEnumerable<T> EnumerateParents<T>(PrototypeId<T> id, bool includeSelf = false) where T : class, IPrototype, IInheritingPrototype
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            if (!TryIndex(id, out var prototype))
                yield break;
            if (includeSelf) yield return prototype;
            if (prototype.Parents == null) yield break;

            var queue = new Queue<string>(prototype.Parents);
            while (queue.TryDequeue(out var prototypeId))
            {
                if (!TryIndex(new PrototypeId<T>(prototypeId), out var parent))
                    yield break;
                yield return parent;
                if (parent.Parents == null) continue;

                foreach (var parentId in parent.Parents)
                {
                    queue.Enqueue(parentId);
                }
            }
        }

        public IEnumerable<IPrototype> EnumerateParents(Type type, string id, bool includeSelf = false)
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            if (!type.IsAssignableTo(typeof(IInheritingPrototype)))
            {
                throw new InvalidOperationException("The provided prototype type is not an inheriting prototype");
            }

            if (!TryIndex(type, id, out var prototype))
                yield break;
            if (includeSelf) yield return prototype;
            var iPrototype = (IInheritingPrototype)prototype;
            if (iPrototype.Parents == null) yield break;

            var queue = new Queue<string>(iPrototype.Parents);
            while (queue.TryDequeue(out var prototypeId))
            {
                if (!TryIndex(type, id, out var parent))
                    continue;
                yield return parent;
                iPrototype = (IInheritingPrototype)parent;
                if (iPrototype.Parents == null) continue;

                foreach (var parentId in iPrototype.Parents)
                {
                    queue.Enqueue(parentId);
                }
            }
        }

        public T Index<T>(PrototypeId<T> id) where T : class, IPrototype
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            try
            {
                return (T)_prototypes[typeof(T)][(string)id];
            }
            catch (KeyNotFoundException)
            {
                if (_prototypeErrors[typeof(T)].TryGetValue((string)id, out var error))
                    throw new UnknownPrototypeException((string)id, typeof(T), error);
                else
                    throw new UnknownPrototypeException((string)id, typeof(T));
            }
        }

        public IPrototype Index(Type type, string id)
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No prototypes have been loaded yet.");
            }

            return _prototypes[type][id];
        }

        public void Clear()
        {
            _prototypeTypes.Clear();
            _prototypes.Clear();
            _prototypeResults.Clear();
            _sortedPrototypes.Clear();
            _sortedPrototypeIndices.Clear();
            _inheritanceTrees.Clear();
            _prototypeExtendedData.Clear();
            _prototypeEventDefs.Clear();
            _prototypeErrors.Clear();
            _eventBus?.ClearEventTables();
        }

        private int SortPrototypesByPriority(Type a, Type b)
        {
            return _prototypePriorities[b].CompareTo(_prototypePriorities[a]);
        }

        protected void ReloadPrototypes(IEnumerable<ResourcePath> filePaths)
        {
#if !FULL_RELEASE
            var changed = new Dictionary<Type, HashSet<string>>();
            foreach (var filePath in filePaths)
            {
                LoadFile(filePath.ToRootedPath(), true, changed);
            }
            ReloadPrototypes(changed);
#endif
        }

        internal void ReloadPrototypes(Dictionary<Type, HashSet<string>> prototypes)
        {
#if !FULL_RELEASE
            var prototypeTypeOrder = prototypes.Keys.ToList();
            prototypeTypeOrder.Sort(SortPrototypesByPriority);

            var pushed = new Dictionary<Type, HashSet<string>>();

            foreach (var type in prototypeTypeOrder)
            {
                if (!type.IsAssignableTo(typeof(IInheritingPrototype)))
                {
                    foreach (var id in prototypes[type])
                    {
                        _prototypes[type][id] = (IPrototype)_serializationManager.Read(type, _prototypeResults[type][id])!;
                    }
                    continue;
                }

                var tree = _inheritanceTrees[type];
                var processQueue = new Queue<string>();
                foreach (var id in prototypes[type])
                {
                    processQueue.Enqueue(id);
                }

                string? previousProtoID = null;
                while (processQueue.TryDequeue(out var id))
                {
                    var pushedSet = pushed.GetOrInsertNew(type);

                    if (tree.TryGetParents(id, out var parents))
                    {
                        var nonPushedParent = false;
                        foreach (var parent in parents)
                        {
                            //our parent has been reloaded and has not been added to the pushedSet yet
                            if (prototypes[type].Contains(parent) && !pushedSet.Contains(parent))
                            {
                                //we re-queue ourselves at the end of the queue
                                processQueue.Enqueue(id);
                                nonPushedParent = true;
                                break;
                            }
                        }
                        if (nonPushedParent) continue;

                        foreach (var parent in parents)
                        {
                            PushInheritance(type, id, parent);
                        }
                    }

                    TryReadPrototype(type, id, _prototypeResults[type][id], ref previousProtoID);

                    pushedSet.Add(id);
                }
            }

            //todo paul i hate it but i am not opening that can of worms in this refactor
            PrototypesReloaded?.Invoke(
                new PrototypesReloadedEventArgs(
                    prototypes
                        .ToDictionary(
                            g => g.Key,
                            g => new PrototypesReloadedEventArgs.PrototypeChangeSet(
                                g.Value
                                .Where(a => _prototypes[g.Key].ContainsKey(a)) // exclude abstract prototypes
                                .ToDictionary(a => a, a => _prototypes[g.Key][a])))));

            // TODO filter by entity prototypes changed
            if (!pushed.ContainsKey(typeof(EntityPrototype))) return;

            var entityPrototypes = _prototypes[typeof(EntityPrototype)];

            foreach (var prototype in pushed[typeof(EntityPrototype)])
            {
                foreach (var metaData in _entityManager.GetAllComponents<MetaDataComponent>()
                    .Where(m => m.EntityPrototype?.ID == prototype))
                {
                    _entityFactory.UpdateEntity(metaData, (EntityPrototype)entityPrototypes[prototype]);
                }
            }
#endif
        }

        public void ResolveResults()
        {
            ResyncInheritance();
            SortAllPrototypes();

            _hasEverBeenReloaded = true;
        }

        public IEnumerable<Type> EnumeratePrototypeTypes()
        {
            var types = _prototypeResults.Keys.ToList();
            types.Sort(SortPrototypesByPriority);
            return types;
        }

        public IEnumerable<PrototypeInheritance> EnumerateInheritanceTree(Type type)
        {
            if (_inheritanceTrees.TryGetValue(type, out var tree))
            {
                var processed = new HashSet<string>();
                var workList = new Queue<string>(tree.RootNodes);

                while (workList.TryDequeue(out var id))
                {
                    processed.Add(id);
                    if (tree.TryGetParents(id, out var parents))
                    {
                        foreach (var parent in parents)
                        {
                            yield return new PrototypeInheritance(type, id, parent);
                        }
                    }

                    if (tree.TryGetChildren(id, out var children))
                    {
                        foreach (var child in children)
                        {
                            var childParents = tree.GetParents(child)!;
                            if (childParents.All(p => processed.Contains(p)))
                                workList.Enqueue(child);
                        }
                    }
                }
            }
        }

        private void ResyncInheritance()
        {
            var types = _prototypeResults.Keys.ToList();
            types.Sort(SortPrototypesByPriority);
            foreach (var type in types)
            {
                if (_inheritanceTrees.TryGetValue(type, out var tree))
                {
                    var processed = new HashSet<string>();
                    var workList = new Queue<string>(tree.RootNodes);

                    while (workList.TryDequeue(out var id))
                    {
                        processed.Add(id);
                        if (tree.TryGetParents(id, out var parents))
                        {
                            foreach (var parent in parents)
                            {
                                PushInheritance(type, id, parent);
                            }
                        }

                        if (tree.TryGetChildren(id, out var children))
                        {
                            foreach (var child in children)
                            {
                                var childParents = tree.GetParents(child)!;
                                if (childParents.All(p => processed.Contains(p)))
                                    workList.Enqueue(child);
                            }
                        }
                    }
                }

                string? previousProtoID = null;
                foreach (var (id, mapping) in _prototypeResults[type])
                {
                    TryReadPrototype(type, id, mapping, ref previousProtoID);
                }
            }
        }

        public void RegisterEvents()
        {
            _eventBus.ClearEventTables();

            var registered = 0;

            var flags = BindingFlags.Instance | BindingFlags.Public;
            var eventBusType = _eventBus.GetType();
            var subscribeValue = eventBusType.GetMethod("SubscribeEventValue", flags)!;
            var subscribeRef = eventBusType.GetMethod("SubscribeEventRef", flags)!;

            foreach (var (prototypeType, eventDefLists) in _prototypeEventDefs)
            {
                foreach (var (prototypeId, eventDefs) in eventDefLists)
                {
                    foreach (var eventDef in eventDefs)
                    {
                        if (!typeof(IEntitySystem).IsAssignableFrom(eventDef.EntitySystemType))
                        {
                            var errorMessage = $"Cannot register events for non-entity system {eventDef.EntitySystemType}! ({prototypeId})";
                            Logger.ErrorS("Serv3", errorMessage);
                            continue;
                        }

                        var target = _entitySystemManager.GetEntitySystem(eventDef.EntitySystemType);

                        Type handlerType;
                        MethodInfo subMethod;
                        var isByRef = eventDef.EventType.HasCustomAttribute<ByRefEventAttribute>();

                        if (isByRef)
                        {
                            handlerType = typeof(PrototypeEventRefHandler<,>);
                            subMethod = subscribeRef;
                        }
                        else
                        {
                            handlerType = typeof(PrototypeEventHandler<,>);
                            subMethod = subscribeValue;
                        }

                        var handlerTypeGeneric = handlerType.MakeGenericType(prototypeType, eventDef.EventType);
                        var handlerMethod = handlerTypeGeneric.GetMethod("Invoke", flags)!;

                        var methodDef = eventDef.EntitySystemType.GetMethod(eventDef.MethodName, flags);
                        if (methodDef == null)
                        {
                            var handlerMethodSignature = $"void {eventDef.MethodName}{PrettyPrint.PrintMethodSignature(handlerMethod, returnType: false, name: false)}";
                            var errorMessage = $"Method {handlerMethodSignature} on entity system {eventDef.EntitySystemType} not found! ({prototypeId})";
                            Logger.ErrorS("Serv3", errorMessage);
                            continue;
                        }

                        try
                        {
                            var handler = methodDef.CreateDelegate(handlerTypeGeneric, target);

                            // Call the subscription method on the prototype event bus based on refness.
                            var genericSubMethod = subMethod.MakeGenericMethod(prototypeType, eventDef.EventType);
                            genericSubMethod.Invoke(_eventBus, new object[] { prototypeId, handler, eventDef.Priority });

                            registered++;
                        }
                        catch (ArgumentException ex)
                        {
                            // "Cannot bind to the target method because its signature is not compatible with that of the delegate type."
                            var errorMessage = $"Method {eventDef.EntitySystemType}.{eventDef.MethodName} is not compatible with delegate type.\nDelegate: {PrettyPrint.PrintMethodSignature(handlerMethod)}\nPassed method: {PrettyPrint.PrintMethodSignature(methodDef)}\nException:{ex}";
                            Logger.ErrorS("Serv3", errorMessage);
                        }
                    }
                }
            }

            Logger.InfoS("Serv3", $"Registered {registered} prototype events.");

            // null means all prototypes have been reloaded.
            PrototypesReloaded?.Invoke(new PrototypesReloadedEventArgs(null));
        }

        private void SortAllPrototypes()
        {
            // Sort all prototypes according to topological sort order.
            _sortedPrototypes.Clear();
            _sortedPrototypeIndices.Clear();
            foreach (var (prototypeType, protos) in _prototypes)
            {
                var nodes = TopologicalSort.FromBeforeAfter(
                    protos.Values,
                    p => p.ID,
                    p => p,
                    p => _prototypeOrdering[prototypeType].GetValueOrDefault(p.ID)?.Before ?? Array.Empty<string>(),
                    p => _prototypeOrdering[prototypeType].GetValueOrDefault(p.ID)?.After ?? Array.Empty<string>(),
                    allowMissing: true);

                _sortedPrototypes[prototypeType] = TopologicalSort.Sort(nodes).ToList();
                BuildPrototypeIndices(prototypeType);
            }
        }

        private void TryReadPrototype(Type type, string id, MappingDataNode mapping, ref string? previousProtoID)
        {
            if (mapping.TryGet<ValueDataNode>(AbstractDataFieldAttribute.Name, out var abstractNode) && abstractNode.AsBool())
                return;
            try
            {
                var prototype = (IPrototype)_serializationManager.Read(type, mapping)!;
                _prototypes[type][id] = prototype;

                _prototypeOrdering[type].Remove(prototype.ID);
                _prototypeExtendedData[type].Remove(prototype.ID);
                _prototypeEventDefs[type].Remove(prototype.ID);
                _prototypeErrors[type].Remove(prototype.ID);

                // TODO!
                var filename = "???";

                if (mapping.TryGet<MappingDataNode>("ordering", out var orderingNode))
                {
                    ParseOrdering(previousProtoID, type, prototype, orderingNode);
                }

                if (mapping.TryGet<SequenceDataNode>("extendedData", out var extDataNode))
                {
                    ParseExtendedData(filename, type, prototype.ID, extDataNode);
                }

                if (mapping.TryGet<SequenceDataNode>("events", out var eventsNode))
                {
                    ParseEvents(filename, type, prototype.ID, eventsNode);
                }

                previousProtoID = prototype.ID;
            }
            catch (Exception e)
            {
                Logger.ErrorS("PROTO", $"Reading {type}({id}) threw the following exception: {e}");
                _prototypeErrors[type][id] = e;
            }
        }

        private void PushInheritance(Type type, string id, string parent)
        {
            _prototypeResults[type][id] = _serializationManager.PushCompositionWithGenericNode(type,
                new[] { _prototypeResults[type][parent] }, _prototypeResults[type][id]);
        }

        /// <inheritdoc />
        public void LoadDirectory(ResourcePath path, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null)
        {
            _hasEverBeenReloaded = true;
            var streams = Resources.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "yml" && !filePath.Filename.StartsWith("."));

            foreach (var resourcePath in streams)
            {
                LoadFile(resourcePath, overwrite, changed);
            }
        }

        public Dictionary<string, HashSet<ErrorNode>> ValidateDirectory(ResourcePath path)
        {
            var streams = Resources.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "yml" && !filePath.Filename.StartsWith("."));

            var dict = new Dictionary<string, HashSet<ErrorNode>>();
            foreach (var resourcePath in streams)
            {
                using var reader = ReadFile(resourcePath);

                if (reader == null)
                {
                    continue;
                }

                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                for (var i = 0; i < yamlStream.Documents.Count; i++)
                {
                    YamlDocument document = yamlStream.Documents[i];
                    var rootNode = (YamlSequenceNode)document.RootNode;
                    foreach (YamlMappingNode node in rootNode.Cast<YamlMappingNode>())
                    {
                        var type = node.GetNode("type").AsString();
                        if (!_prototypeTypes.TryGetValue(type, out var prototypeType))
                        {
                            throw new PrototypeLoadException($"Unknown prototype type: '{type}'", resourcePath.ToString(), node);
                        }

                        var prototypeID = node.GetNode("id").AsString();
                        var mapping = node.ToDataNodeCast<MappingDataNode>();
                        mapping.Remove("type");
                        var errorNodes = _serializationManager.ValidateNode(_prototypeTypes[type], mapping).GetErrors()
                            .ToHashSet();
                        if (errorNodes.Count == 0) continue;
                        if (!dict.TryGetValue(resourcePath.ToString(), out var hashSet))
                            dict[resourcePath.ToString()] = new HashSet<ErrorNode>();
                        dict[resourcePath.ToString()].UnionWith(errorNodes);

                        if (node.TryGetNode("extendedData", out var extDataNode) && extDataNode is YamlSequenceNode extDataSequenceNode)
                        {
                            ValidateExtendedData(resourcePath.ToString(), prototypeType, prototypeID, extDataSequenceNode, dict);
                        }

                        if (node.TryGetNode("events", out var eventsNode) && eventsNode is YamlSequenceNode eventsSequenceNode)
                        {
                            ValidateEvents(resourcePath.ToString(), prototypeType, prototypeID, eventsSequenceNode, dict);
                        }
                    }
                }
            }

            return dict;
        }

        private void ValidateExtendedData(string filename, Type prototypeType, string prototypeID, YamlSequenceNode extDataSequenceNode, Dictionary<string, HashSet<ErrorNode>> errors)
        {
            var extDataIfaceType = typeof(IPrototypeExtendedData<>)
                .MakeGenericType(prototypeType);

            foreach (var child in extDataSequenceNode.Children.Cast<YamlMappingNode>())
            {
                var childNode = child.ToDataNode();
                if (!child.TryGetNode("type", out var extDataTypeNode))
                {
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(childNode, $"Extended data entry is missing 'type' property"));
                    continue;
                }

                var extDataTypeStr = extDataTypeNode.AsString();
                if (!_reflectionManager.TryLooseGetType(extDataTypeStr, out var extDataType))
                {
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(childNode, $"Unable to find type ending with '{extDataTypeStr}'"));
                    continue;
                }

                if (!extDataIfaceType.IsAssignableFrom(extDataType))
                {
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(childNode, $"Extended data of type '{extDataType}' cannot apply to prototype of type '{prototypeType}', as it does not implement '{extDataIfaceType}'"));
                }
            }
        }

        private void ValidateEvents(string filename, Type prototypeType, string prototypeID, YamlSequenceNode eventsSequenceNode, Dictionary<string, HashSet<ErrorNode>> errors)
        {
            var sequence = eventsSequenceNode.ToDataNodeCast<SequenceDataNode>();
            var errorNodes = _serializationManager.ValidateNode<List<PrototypeEventHandlerDef>>(sequence).GetErrors()
                .ToHashSet();
            if (errorNodes.Count != 0)
            {
                errors.GetValueOrInsert(filename).AddRange(errorNodes);
                return;
            }

            var flags = BindingFlags.Instance | BindingFlags.Public;

            foreach (var node in sequence.Sequence)
            {
                var eventDef = _serializationManager.Read<PrototypeEventHandlerDef>(node);

                if (!typeof(IEntitySystem).IsAssignableFrom(eventDef.EntitySystemType))
                {
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(node, $"Cannot register events for non-entity system {eventDef.EntitySystemType}. ({prototypeID})"));
                    continue;
                }

                var methodDef = eventDef.EntitySystemType.GetMethod(eventDef.MethodName, flags);
                if (methodDef == null)
                {
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(node, $"Method {eventDef.EntitySystemType}.{eventDef.MethodName}(...) not found. ({prototypeID})"));
                    continue;
                }

                var target = _entitySystemManager.GetEntitySystem(eventDef.EntitySystemType);

                Type handlerType;
                var isByRef = eventDef.EventType.HasCustomAttribute<ByRefEventAttribute>();

                if (isByRef)
                {
                    handlerType = typeof(PrototypeEventRefHandler<,>);
                }
                else
                {
                    handlerType = typeof(PrototypeEventHandler<,>);
                }

                var handlerTypeGeneric = handlerType.MakeGenericType(prototypeType, eventDef.EventType);

                try
                {
                    methodDef.CreateDelegate(handlerTypeGeneric, target);
                }
                catch (ArgumentException ex)
                {
                    // "Cannot bind to the target method because its signature is not compatible with that of the delegate type."
                    var handlerMethod = handlerTypeGeneric.GetMethod("Invoke", flags)!;
                    var errorMessage = $"Method {eventDef.EntitySystemType}.{eventDef.MethodName} is not compatible with delegate type. --- Delegate: {PrettyPrint.PrintMethodSignature(handlerMethod)} - Passed method: {PrettyPrint.PrintMethodSignature(methodDef)}";
                    errors.GetValueOrInsert(filename).Add(new ErrorNode(node, errorMessage));
                    continue;
                }
            }
        }

        private TextReader? ReadFile(ResourcePath file, bool @throw = true)
        {
            var retries = 0;

            // This might be shit-code, but its pjb-responded-idk-when-asked shit-code.
            while (true)
            {
                try
                {
                    var reader = new StringReader(Resources.ContentFileReadAllText(file, EncodingHelpers.UTF8));
                    return reader;
                }
                catch (IOException e)
                {
                    if (retries > 10)
                    {
                        if (@throw)
                        {
                            throw;
                        }

                        Logger.Error($"Error reloading prototypes in file {file}.", e);
                        return null;
                    }

                    retries++;
                    Thread.Sleep(10);
                }
            }
        }

        public void LoadFile(ResourcePath file, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null)
        {
            var changedPrototypes = new HashSet<IPrototype>();

            try
            {
                using var reader = ReadFile(file, !overwrite);

                if (reader == null)
                {
                    return;
                }

                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                LoadedData?.Invoke(yamlStream, file.ToString());

                for (var i = 0; i < yamlStream.Documents.Count; i++)
                {
                    try
                    {
                        LoadFromDocument(yamlStream.Documents[i], overwrite, changed, file.ToString());
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorS("eng", $"Exception whilst loading prototypes from {file}#{i}:\n{e}");
                    }
                }
            }
            catch (YamlException e)
            {
                var sawmill = Logger.GetSawmill("eng");
                sawmill.Error("YamlException whilst loading prototypes from {0}: {1}", file, e.Message);
            }
        }

        public void LoadFromStream(TextReader stream, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null)
        {
            var changedPrototypes = new List<IPrototype>();
            _hasEverBeenReloaded = true;
            var yaml = new YamlStream();
            yaml.Load(stream);

            for (var i = 0; i < yaml.Documents.Count; i++)
            {
                var document = yaml.Documents[i];
                try
                {
                    LoadFromDocument(document, overwrite, changed);
                }
                catch (Exception e)
                {
                    throw new PrototypeLoadException($"Failed to load prototypes from document#{i}", e, "[anonymous]");
                }
            }

            LoadedData?.Invoke(yaml, "anonymous prototypes YAML stream");
        }

        public void LoadString(string str, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null)
        {
            LoadFromStream(new StringReader(str), overwrite, changed);
        }

        public void RemoveString(string prototypes)
        {
            var reader = new StringReader(prototypes);
            var yaml = new YamlStream();

            yaml.Load(reader);

            foreach (var document in yaml.Documents)
            {
                var root = (YamlSequenceNode)document.RootNode;
                foreach (var node in root.Cast<YamlMappingNode>())
                {
                    var typeString = node.GetNode("type").AsString();
                    var type = _prototypeTypes[typeString];

                    var id = node.GetNode("id").AsString();

                    if (_inheritanceTrees.TryGetValue(type, out var tree))
                    {
                        tree.Remove(id, true);
                    }

                    if (_prototypes.TryGetValue(type, out var prototypeIds))
                    {
                        prototypeIds.Remove(id);
                        _prototypeResults[type].Remove(id);
                    }
                }
            }
        }

        #endregion IPrototypeManager members

        private void ReloadPrototypeTypes()
        {
            Clear();
            foreach (var type in _reflectionManager.GetAllChildren<IPrototype>())
            {
                RegisterType(type);
            }
        }

        private void LoadFromDocument(YamlDocument document, bool overwrite = false, Dictionary<Type, HashSet<string>>? changed = null, string? filename = null)
        {
            var rootNode = (YamlSequenceNode)document.RootNode;
            filename ??= "[anonymous]";

            foreach (var node in rootNode.Cast<YamlMappingNode>())
            {
                var dataNode = node.ToDataNodeCast<MappingDataNode>();
                var type = dataNode.Get<ValueDataNode>("type").Value;

                if (!_prototypeTypes.TryGetValue(type, out var prototypeType))
                {
                    throw new PrototypeLoadException($"Unknown prototype type: '{type}'", filename, node);
                }

                if (!dataNode.TryGet<ValueDataNode>(IdDataFieldAttribute.Name, out var idNode))
                {
                    throw new PrototypeLoadException($"Prototype type {type} is missing an 'id' datafield.");
                }

                if (!overwrite && _prototypes[prototypeType].ContainsKey(idNode.Value))
                {
                    throw new PrototypeLoadException($"Duplicate ID: '{idNode.Value}'");
                }

                BeforePrototypeLoad?.Invoke(prototypeType, type, idNode.Value, dataNode);

                _prototypeResults[prototypeType][idNode.Value] = dataNode;
                if (prototypeType.IsAssignableTo(typeof(IInheritingPrototype)))
                {
                    if (dataNode.TryGet(ParentDataFieldAttribute.Name, out var parentNode))
                    {
                        var parents = _serializationManager.Read<string[]>(parentNode);
                        _inheritanceTrees[prototypeType].Add(idNode.Value, parents);
                    }
                    else
                    {
                        _inheritanceTrees[prototypeType].Add(idNode.Value);
                    }
                }

                if (changed != null)
                {
                    changed.GetOrInsertNew(prototypeType).Add(idNode.Value);
                }
            }
        }

        private void ParseOrdering(string? previousProtoID, Type prototypeType, IPrototype prototype, MappingDataNode orderingMappingNode)
        {
            string[]? before = null;
            string[]? after = null;
            if (orderingMappingNode.TryGet<ValueDataNode>("before", out var orderBeforeNode))
            {
                before = new[] { orderBeforeNode.Value };
            }
            if (orderingMappingNode.TryGet<ValueDataNode>("after", out var orderAfterNode))
            {
                after = new[] { orderAfterNode.Value };
            }

            // Order prototypes sequentially based on their order in the document
            // if no other ordering is specified.
            if (before == null && after == null && previousProtoID != null)
                after = new[] { previousProtoID };

            var ordering = new PrototypeOrderingData(Before: before ?? Array.Empty<string>(), After: after ?? Array.Empty<string>());
            _prototypeOrdering[prototypeType][prototype.ID] = ordering;
        }

        private void ParseExtendedData(string? filename, Type prototypeType, string prototypeID, SequenceDataNode extDataSequenceNode)
        {
            var extDataIfaceType = typeof(IPrototypeExtendedData<>)
                .MakeGenericType(prototypeType);

            foreach (var child in extDataSequenceNode)
            {
                var childMapping = (YamlMappingNode)child.ToYamlNode();
                if (!childMapping.TryGetNode("type", out var extDataTypeNode))
                {
                    throw new PrototypeLoadException($"Extended data entry is missing 'type' property", filename, childMapping);
                }

                var extDataTypeStr = extDataTypeNode.AsString();
                if (!_reflectionManager.TryLooseGetType(extDataTypeStr, out var extDataType))
                {
                    throw new PrototypeLoadException($"Unable to find type ending with '{extDataTypeStr}'", filename, childMapping);
                }

                var extDataMappingNode = childMapping.ToDataNodeCast<MappingDataNode>();
                var extDataRes = _serializationManager.Read(extDataType, extDataMappingNode, skipHook: true);
                var obj = (IPrototypeExtendedData)extDataRes!;

                if (!extDataIfaceType.IsAssignableFrom(obj.GetType()))
                {
                    throw new PrototypeLoadException($"Extended data of type '{obj.GetType()}' cannot apply to prototype of type '{prototypeType}', as it does not implement '{extDataIfaceType}'", filename, childMapping);
                }

                var objs = _prototypeExtendedData[prototypeType].GetValueOrInsert(prototypeID, () => new());
                objs.Add(obj.GetType(), obj);
            }
        }

        private void ParseEvents(string filename, Type prototypeType, string prototypeID, SequenceDataNode eventsSequenceNode)
        {
            _prototypeEventDefs[prototypeType][prototypeID] = _serializationManager.Read<List<PrototypeEventHandlerDef>>(eventsSequenceNode);
        }

        private void BuildPrototypeIndices(Type prototypeType)
        {
            var indices = new Dictionary<string, int>();
            for (var i = 0; i < _sortedPrototypes[prototypeType].Count; i++)
            {
                var proto = _sortedPrototypes[prototypeType][i];
                indices[proto.ID] = i;
            }
            _sortedPrototypeIndices[prototypeType] = indices;
        }

        public bool HasIndex<T>(PrototypeId<T> id) where T : class, IPrototype
            => HasIndex(typeof(T), (string)id);

        public bool HasIndex(Type type, string id)
        {
            if (id == null)
                return false;

            if (!_prototypes.TryGetValue(type, out var index))
            {
                if (_prototypeErrors[type].TryGetValue(id, out var error))
                    throw new UnknownPrototypeException(id, type, error);
                else
                    throw new UnknownPrototypeException(id, type);
            }

            return index.ContainsKey(id);
        }

        public bool TryIndex<T>(PrototypeId<T> id, [NotNullWhen(true)] out T? prototype, bool logMissing = true) where T : class, IPrototype
        {
            var returned = TryIndex(typeof(T), (string)id, out var proto, logMissing);
            prototype = (proto ?? null) as T;
            return returned;
        }

        public bool TryIndex(Type type, string id, [NotNullWhen(true)] out IPrototype? prototype, bool logMissing = true)
        {
            if (id == null)
            {
                if (logMissing)
                    Logger.ErrorS("resolve", $"Can't find prototype {id} of type \"{type}\"!\n{new StackTrace(1, true)}");

                prototype = null;
                return false;
            }

            if (!_prototypes.TryGetValue(type, out var index))
            {
                if (_prototypeErrors[type].TryGetValue(id, out var error))
                    throw new UnknownPrototypeException(id, type, error);
                else
                    throw new UnknownPrototypeException(id, type);
            }

            var found = index.TryGetValue(id, out prototype);
            if (!found && logMissing)
                Logger.ErrorS("resolve", $"Can't find prototype {id} of type \"{type}\"!\n{new StackTrace(1, true)}");

            return found;
        }

        public bool HasMapping<T>(PrototypeId<T> id) where T : class, IPrototype
        {
            if (!_prototypeResults.TryGetValue(typeof(T), out var index))
            {
                if (_prototypeErrors[typeof(T)].TryGetValue((string)id, out var error))
                    throw new UnknownPrototypeException((string)id, typeof(T), error);
                else
                    throw new UnknownPrototypeException((string)id, typeof(T));
            }

            return index.ContainsKey((string)id);
        }

        public bool TryGetMapping(Type type, string id, [NotNullWhen(true)] out MappingDataNode? mappings)
        {
            var ret = _prototypeResults[type].TryGetValue(id, out var originalMappings);
            mappings = originalMappings?.Copy();
            return ret;
        }

        /// <inheritdoc />
        public bool HasVariant(string variant)
        {
            return _prototypeTypes.ContainsKey(variant);
        }

        /// <inheritdoc />
        public Type GetVariantType(string variant)
        {
            return _prototypeTypes[variant];
        }

        /// <inheritdoc />
        public bool TryGetVariantType(string variant, [NotNullWhen(true)] out Type? prototype)
        {
            return _prototypeTypes.TryGetValue(variant, out prototype);
        }

        /// <inheritdoc />
        public bool TryGetVariantFrom(Type type, [NotNullWhen(true)] out string? variant)
        {
            variant = null;

            // If the type doesn't implement IPrototype, this fails.
            if (!(typeof(IPrototype).IsAssignableFrom(type)))
                return false;

            var attribute = (PrototypeAttribute?)Attribute.GetCustomAttribute(type, typeof(PrototypeAttribute));

            // If the prototype type doesn't have the attribute, this fails.
            if (attribute == null)
                return false;

            // If the variant isn't registered, this fails.
            if (!HasVariant(attribute.Type))
                return false;

            variant = attribute.Type;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetVariantFrom<T>([NotNullWhen(true)] out string? variant) where T : class, IPrototype
        {
            return TryGetVariantFrom(typeof(T), out variant);
        }

        /// <inheritdoc />
        public bool TryGetVariantFrom(IPrototype prototype, [NotNullWhen(true)] out string? variant)
        {
            return TryGetVariantFrom(prototype.GetType(), out variant);
        }

        /// <inheritdoc />
        public TExt GetExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            if (!TryGetExtendedData<TProto, TExt>(id, out var data))
                throw new KeyNotFoundException($"Extended data {typeof(TExt)} for {typeof(TProto)}:{id} not found.");

            return data;
        }

        /// <inheritdoc />
        public TExt GetExtendedData<TProto, TExt>(TProto proto)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            if (!TryGetExtendedData<TProto, TExt>(proto, out var data))
                throw new KeyNotFoundException($"Extended data {typeof(TExt)} for {typeof(TProto)}:{proto.ID} not found.");

            return data;
        }

        /// <inheritdoc />
        public IPrototypeExtendedData GetExtendedData(Type protoType, Type extType, string id)
        {
            if (!TryGetExtendedData(protoType, extType, id, out var data))
                throw new KeyNotFoundException($"Extended data {extType} for {protoType}:{id} not found.");

            return data;
        }

        /// <inheritdoc />
        public bool HasExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            return HasExtendedData(typeof(TProto), typeof(TExt), (string)id);
        }

        /// <inheritdoc />
        public bool HasExtendedData<TProto, TExt>(TProto proto)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            return HasExtendedData(typeof(TProto), typeof(TExt), proto.ID);
        }

        /// <inheritdoc />
        public bool HasExtendedData(Type protoType, Type extType, string id)
        {
            if (!_prototypeExtendedData.TryGetValue(protoType, out var extData))
                return false;

            if (!extData.TryGetValue(id, out var objs))
                return false;

            return objs.ContainsKey(extType);
        }

        /// <inheritdoc />
        public bool TryGetExtendedData<TProto, TExt>(PrototypeId<TProto> id, [NotNullWhen(true)] out TExt? data)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            if (!TryGetExtendedData(typeof(TProto), typeof(TExt), (string)id, out var obj))
            {
                data = null;
                return false;
            }

            data = (TExt)obj;
            return true;
        }

        public bool TryGetExtendedData<TProto, TExt>(TProto proto, [NotNullWhen(true)] out TExt? data)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData<TProto>
        {
            if (!TryGetExtendedData(typeof(TProto), typeof(TExt), proto.ID, out var obj))
            {
                data = null;
                return false;
            }

            data = (TExt)obj;
            return true;
        }

        /// <inheritdoc />
        public bool TryGetExtendedData(Type protoType, Type extType, string id, [NotNullWhen(true)] out IPrototypeExtendedData? obj)
        {
            if (!typeof(IPrototype).IsAssignableFrom(protoType))
                throw new ArgumentException($"{protoType} is not a prototype type.");
            if (!typeof(IPrototypeExtendedData).IsAssignableFrom(extType))
                throw new ArgumentException($"{extType} is not an extended data type.");

            obj = null;
            if (!_prototypeExtendedData.TryGetValue(protoType, out var extData))
                return false;

            if (!extData.TryGetValue(id, out var objs))
                return false;

            return objs.TryGetValue(extType, out obj);
        }

        public void RegisterType<T>() where T : IPrototype
        {
            RegisterType(typeof(T));
        }

        /// <inheritdoc />
        public void RegisterType(Type type)
        {
            if (!(typeof(IPrototype).IsAssignableFrom(type)))
                throw new InvalidOperationException("Type must implement IPrototype.");

            var attribute = (PrototypeAttribute?)Attribute.GetCustomAttribute(type, typeof(PrototypeAttribute));

            if (attribute == null)
            {
                throw new InvalidImplementationException(type,
                    typeof(IPrototype),
                    "No " + nameof(PrototypeAttribute) + " to give it a type string.");
            }

            if (_prototypeTypes.ContainsKey(attribute.Type))
            {
                throw new InvalidImplementationException(type,
                    typeof(IPrototype),
                    $"Duplicate prototype type ID: {attribute.Type}. Current: {_prototypeTypes[attribute.Type]}");
            }

            ValidatePrototypeTypeAttributes(type);

            _prototypeTypes[attribute.Type] = type;
            _prototypePriorities[type] = attribute.LoadPriority;

            if (typeof(IPrototype).IsAssignableFrom(type))
            {
                _prototypes[type] = new Dictionary<string, IPrototype>();
                _prototypeResults[type] = new Dictionary<string, MappingDataNode>();
                if (typeof(IInheritingPrototype).IsAssignableFrom(type))
                    _inheritanceTrees[type] = new MultiRootInheritanceGraph<string>();
                _prototypeOrdering[type] = new Dictionary<string, PrototypeOrderingData>();
                _prototypeExtendedData[type] = new Dictionary<string, Dictionary<Type, IPrototypeExtendedData>>();
                _prototypeEventDefs[type] = new Dictionary<string, List<PrototypeEventHandlerDef>>();
                _prototypeErrors[type] = new Dictionary<string, Exception>();
            }
        }

        private void ValidatePrototypeTypeAttributes(Type type)
        {
            var foundIdAttribute = false;
            var foundParentAttribute = false;
            var foundAbstractAttribute = false;
            foreach (var info in type.GetAllPropertiesAndFields())
            {
                var hasId = info.HasAttribute<IdDataFieldAttribute>();
                var hasParent = info.HasAttribute<ParentDataFieldAttribute>();
                if (hasId)
                {
                    if (foundIdAttribute)
                        throw new InvalidImplementationException(type,
                            typeof(IPrototype),
                            $"Found two {nameof(IdDataFieldAttribute)}");

                    foundIdAttribute = true;
                }

                if (hasParent)
                {
                    if (foundParentAttribute)
                        throw new InvalidImplementationException(type,
                            typeof(IInheritingPrototype),
                            $"Found two {nameof(ParentDataFieldAttribute)}");

                    foundParentAttribute = true;
                }

                if (hasId && hasParent)
                    throw new InvalidImplementationException(type,
                        typeof(IPrototype),
                        $"Prototype {type} has the Id- & ParentDatafield on single member {info.Name}");

                if (info.HasAttribute<AbstractDataFieldAttribute>())
                {
                    if (foundAbstractAttribute)
                        throw new InvalidImplementationException(type,
                            typeof(IInheritingPrototype),
                            $"Found two {nameof(AbstractDataFieldAttribute)}");

                    foundAbstractAttribute = true;
                }
            }

            if (!foundIdAttribute)
                throw new InvalidImplementationException(type,
                    typeof(IPrototype),
                    $"Did not find any member annotated with the {nameof(IdDataFieldAttribute)}");

            if (type.IsAssignableTo(typeof(IInheritingPrototype)) && (!foundParentAttribute || !foundAbstractAttribute))
                throw new InvalidImplementationException(type,
                    typeof(IInheritingPrototype),
                    $"Did not find any member annotated with the {nameof(ParentDataFieldAttribute)} and/or {nameof(AbstractDataFieldAttribute)}");
        }

        public IPrototypeComparer<T> GetComparator<T>() where T : class, IPrototype
        {
            return new PrototypeComparator<T>(this);
        }

        public event Action<YamlStream, string>? LoadedData;
        public event Action<PrototypesReloadedEventArgs>? PrototypesReloaded;
        public event BeforePrototypeLoadDelegate? BeforePrototypeLoad;

        /// <summary>
        /// Orders prototypes and prototype IDs by prototype ordering.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public sealed class PrototypeComparator<T> : IPrototypeComparer<T>
            where T : class, IPrototype
        {
            private readonly Dictionary<string, int> _indices;

            public PrototypeComparator(PrototypeManager protos)
            {
                _indices = protos._sortedPrototypeIndices[typeof(T)];
            }

            public int Compare(PrototypeId<T>? x, PrototypeId<T>? y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                return _indices[(string)x].CompareTo(_indices[(string)y]);
            }

            public int Compare(T? x, T? y)
            {
                if (x == null && y == null)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return 1;

                return _indices[x.ID].CompareTo(_indices[y.ID]);
            }
        }
    }

    // TODO dedup yaml-related exceptions
    [Serializable]
    public class PrototypeLoadException : Exception
    {
        public override string Message
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(base.Message);

                if (Filename != null)
                {
                    sb.Append($" at {Filename}");

                    if (Node != null)
                    {
                        sb.Append($", line {Node.Start.Line}, column {Node.Start.Column}");
                    }
                }

                return sb.ToString();
            }
        }

        public readonly string? Filename;
        public readonly YamlNode? Node;

        public PrototypeLoadException()
        {
        }

        public PrototypeLoadException(string message, string? filename = null, YamlNode? node = null) : base(message)
        {
            Filename = filename;
            Node = node;
        }

        public PrototypeLoadException(string message, Exception inner, string? filename = null) : base(message, inner)
        {
            Filename = filename;
        }

        public PrototypeLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("filename", Filename, typeof(string));
            info.AddValue("node", Node, typeof(YamlNode));
        }
    }

    [Serializable]
    public class UnknownPrototypeException : Exception
    {
        public override string Message => $"Unknown prototype of type {Type?.Name}: {Prototype}";
        public readonly string? Prototype;
        public readonly Type? Type;

        public UnknownPrototypeException(string? prototype, Type? type)
        {
            Prototype = prototype;
            Type = type;
        }

        public UnknownPrototypeException(string? prototype, Type? type, Exception innerException) : base(null, innerException)
        {
            Prototype = prototype;
            Type = type;
        }

        public UnknownPrototypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Prototype = (string?)info.GetValue("prototype", typeof(string));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("prototype", Prototype, typeof(string));
        }
    }

    /// <param name="ByType"><c>null</c> means all prototypes were loaded for the first time.</param>
    public sealed record PrototypesReloadedEventArgs(IReadOnlyDictionary<Type, PrototypesReloadedEventArgs.PrototypeChangeSet>? ByType)
    {
        public sealed record PrototypeChangeSet(IReadOnlyDictionary<string, IPrototype> Modified);

        public bool TryGetModified<T>(IPrototypeManager protoMan, [NotNullWhen(true)] out IEnumerable<T>? result)
            where T : class, IPrototype
        {
            if (ByType == null)
            {
                result = protoMan.EnumeratePrototypes<T>();
                return true;
            }

            if (ByType.TryGetValue(typeof(T), out var changeSet))
            {
                result = changeSet.Modified.Values.Cast<T>();
                return true;
            }

            result = null;
            return false;
        }
    }

    public interface IPrototypeComparer<T> : IComparer<PrototypeId<T>?>, IComparer<T?>
        where T : class, IPrototype
    {
    }
}

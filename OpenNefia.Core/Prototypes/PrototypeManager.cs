using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using ICSharpCode.Decompiler.TypeSystem;
using JetBrains.Annotations;
using NLua;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.IoC.Exceptions;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Utility;
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
        bool TryIndex<T>(PrototypeId<T> id, [NotNullWhen(true)] out T? prototype) where T : class, IPrototype;
        bool TryIndex(Type type, string id, [NotNullWhen(true)] out IPrototype? prototype);

        /// <summary>
        /// Index for a <see cref="IPrototype"/>'s extended data.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the type of prototype is not registered.
        /// </exception>
        TExt GetExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData;

        /// <summary>
        /// Index for a <see cref="IPrototype"/>'s extended data.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the ID does not exist or the type of prototype is not registered.
        /// </exception>
        IPrototypeExtendedData GetExtendedData(Type protoType, Type extType, string id);

        bool HasExtendedData<TProto, TExt>(PrototypeId<TProto> id)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData;
        bool HasExtendedData(Type protoType, Type extType, string id);
        bool TryGetExtendedData<TProto, TExt>(PrototypeId<TProto> id, [NotNullWhen(true)] out TExt? data)
            where TProto : class, IPrototype
            where TExt : class, IPrototypeExtendedData;
        bool TryGetExtendedData(Type protoType, Type extType, string id, [NotNullWhen(true)] out IPrototypeExtendedData? data);

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
        List<IPrototype> LoadDirectory(ResourcePath path, bool overwrite = false);

        Dictionary<string, HashSet<ErrorNode>> ValidateDirectory(ResourcePath path);

        List<IPrototype> LoadFromStream(TextReader stream, bool overwrite = false);

        List<IPrototype> LoadString(string str, bool overwrite = false);

        void RemoveString(string prototypes);

        /// <summary>
        /// Clear out all prototypes and reset to a blank slate.
        /// </summary>
        void Clear();

        /// <summary>
        /// Syncs all inter-prototype data. Call this when operations adding new prototypes are done.
        /// </summary>
        void Resync();

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
        ///     This does NOT fire on initial prototype load.
        /// </remarks>
        event Action<PrototypesReloadedEventArgs> PrototypesReloaded;

        /// <summary>
        ///     Fired before each prototype node is loaded, to allow transforming it.
        /// </summary>
        event Action<MappingDataNode> BeforePrototypeLoad;
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IPrototypeExtendedData
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
        // For avoiding reparsing in unit tests
        PrototypeManagerCache Cache { get; }

        /// <summary>
        /// Loads from set of previously cached parsing results.
        /// </summary>
        List<IPrototype> LoadFromCachedResults(PrototypeManagerCache cache);
    }

    public record PrototypeOrderingData(string[] Before, string[] After);

    public sealed record PrototypeManagerCache(
        Dictionary<Type, Dictionary<string, DeserializationResult>> PrototypeResults,
        Dictionary<Type, Dictionary<string, PrototypeOrderingData>> PrototypeOrdering,
        Dictionary<Type, Dictionary<string, Dictionary<Type, IPrototypeExtendedData>>> PrototypeExtendedData,
        Dictionary<Type, Dictionary<string, List<PrototypeEventHandlerDef>>> PrototypeEventDefs);

    public sealed partial class PrototypeManager : IPrototypeManagerInternal
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IResourceManager Resources = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IEntityFactory _entityFactory = default!;

        private readonly Dictionary<string, Type> _prototypeTypes = new();
        private readonly Dictionary<Type, int> _prototypePriorities = new();

        private PrototypeEventBus _eventBus = null!;

        /// <inheritdoc />
        public IPrototypeEventBus EventBus => _eventBus;

        private bool _initialized;
        private bool _hasEverBeenReloaded;

        #region IPrototypeManager members

        private readonly Dictionary<Type, Dictionary<string, IPrototype>> _prototypes = new();
        private readonly Dictionary<Type, Dictionary<string, DeserializationResult>> _prototypeResults = new();
        private readonly Dictionary<Type, PrototypeInheritanceTree> _inheritanceTrees = new();
        private readonly Dictionary<Type, Dictionary<string, PrototypeOrderingData>> _prototypeOrdering = new();
        private readonly Dictionary<Type, List<IPrototype>> _sortedPrototypes = new();
        private readonly Dictionary<Type, Dictionary<string, Dictionary<Type, IPrototypeExtendedData>>> _prototypeExtendedData = new();
        private readonly Dictionary<Type, Dictionary<string, List<PrototypeEventHandlerDef>>> _prototypeEventDefs = new();

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

        // For avoiding reparsing in unit tests
        public PrototypeManagerCache Cache => new(Deepcopy(_prototypeResults),
            Deepcopy(_prototypeOrdering),
            Deepcopy(_prototypeExtendedData),
            Deepcopy(_prototypeEventDefs));

        public void Initialize()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"{nameof(PrototypeManager)} has already been initialized.");
            }

            _initialized = true;
            ReloadPrototypeTypes();

            _eventBus = new PrototypeEventBus(this);

            _graphics.OnWindowFocused += WindowFocusedChanged;

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
            _inheritanceTrees.Clear();
            _prototypeExtendedData.Clear();
            _prototypeEventDefs.Clear();
            _eventBus?.ClearEventTables();
        }

        private int SortPrototypesByPriority(Type a, Type b)
        {
            return _prototypePriorities[b].CompareTo(_prototypePriorities[a]);
        }

        private void ReloadPrototypes(IEnumerable<ResourcePath> filePaths)
        {
#if !FULL_RELEASE
            var changed = filePaths.SelectMany(f => LoadFile(f.ToRootedPath(), true)).ToList();
            ReloadPrototypes(changed);
#endif
        }

        internal void ReloadPrototypes(List<IPrototype> prototypes)
        {
#if !FULL_RELEASE
            prototypes.Sort((a, b) => SortPrototypesByPriority(a.GetType(), b.GetType()));

            var pushed = new Dictionary<Type, HashSet<string>>();

            foreach (var prototype in prototypes)
            {
                if (prototype is not IInheritingPrototype inheritingPrototype) continue;
                var type = prototype.GetType();
                if (!pushed.ContainsKey(type)) pushed[type] = new HashSet<string>();
                var baseNode = prototype.ID;

                if (pushed[type].Contains(baseNode))
                {
                    continue;
                }

                var tree = _inheritanceTrees[type];
                var currentNode = inheritingPrototype.Parent;

                if (currentNode == null)
                {
                    PushInheritance(type, baseNode, null, pushed[type]);
                    continue;
                }

                while (true)
                {
                    var parent = tree.GetParent(currentNode);

                    if (parent == null)
                    {
                        break;
                    }

                    baseNode = currentNode;
                    currentNode = parent;
                }

                PushInheritance(type, currentNode, baseNode, null, pushed[type]);
            }

            PrototypesReloaded?.Invoke(
                new PrototypesReloadedEventArgs(
                    prototypes
                        .GroupBy(p => p.GetType())
                        .ToDictionary(
                            g => g.Key,
                            g => new PrototypesReloadedEventArgs.PrototypeChangeSet(
                                g.ToDictionary(a => a.ID, a => a)))));

            // TODO filter by entity prototypes changed
            if (pushed.ContainsKey(typeof(EntityPrototype)))
            {
                var entityPrototypes = _prototypes[typeof(EntityPrototype)];

                foreach (var prototype in pushed[typeof(EntityPrototype)])
                {
                    foreach (var metaData in _entityManager.GetAllComponents<MetaDataComponent>()
                        .Where(m => m.EntityPrototype?.ID == prototype))
                    {
                        _entityFactory.UpdateEntity(metaData, (EntityPrototype)entityPrototypes[prototype]);
                    }
                }
            }
#endif
        }

        public void Resync()
        {
            ResyncInheritance();
            SortAllPrototypes();

            _hasEverBeenReloaded = true;
        }

        private void ResyncInheritance()
        {
            var trees = _inheritanceTrees.Keys.ToList();
            trees.Sort(SortPrototypesByPriority);
            foreach (var type in trees)
            {
                var tree = _inheritanceTrees[type];
                foreach (var baseNode in tree.BaseNodes)
                {
                    PushInheritance(type, baseNode, null, new HashSet<string>());
                }

                // Go over all prototypes and double check that their parent actually exists.
                var typePrototypes = _prototypes[type];
                foreach (var (id, proto) in typePrototypes)
                {
                    var iProto = (IInheritingPrototype)proto;

                    var parent = iProto.Parent;
                    if (parent != null && !typePrototypes.ContainsKey(parent!))
                    {
                        Logger.ErrorS("Serv3", $"{iProto.GetType().Name} '{id}' has invalid parent: {parent}");
                    }
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
                            Logger.ErrorS("Serv3", $"Cannot register events for non-entity system {eventDef.EntitySystemType}! ({prototypeId})");
                            continue;
                        }

                        var methodDef = eventDef.EntitySystemType.GetMethod(eventDef.MethodName, flags);
                        if (methodDef == null)
                        {
                            Logger.ErrorS("Serv3", $"Method {eventDef.EntitySystemType}.{eventDef.MethodName}(...) not found! ({prototypeId})");
                            continue;
                        }

                        var target = EntitySystem.Get(eventDef.EntitySystemType);

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

                        var handler = methodDef.CreateDelegate(handlerType
                            .MakeGenericType(prototypeType, eventDef.EventType), target);

                        var genericSubMethod = subMethod.MakeGenericMethod(prototypeType, eventDef.EventType);
                        genericSubMethod.Invoke(_eventBus, new object[] { prototypeId, handler, eventDef.Priority });

                        registered++;
                    }
                }
            }

            Logger.InfoS("Serv3", $"Registered {registered} prototype events.");
        }

        private void SortAllPrototypes()
        {
            // Sort all prototypes according to topological sort order.
            _sortedPrototypes.Clear();
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
            }
        }

        public void PushInheritance(Type type, string id, string child, DeserializationResult? baseResult,
            HashSet<string> changed)
        {
            changed.Add(id);

            var myRes = _prototypeResults[type][id];
            var newResult = baseResult != null ? myRes.PushInheritanceFrom(baseResult) : myRes;

            PushInheritance(type, child, newResult, changed);

            newResult.CallAfterDeserializationHook();
            var populatedRes =
                _serializationManager.PopulateDataDefinition(_prototypes[type][id], (IDeserializedDefinition)newResult);
            _prototypes[type][id] = (IPrototype)populatedRes.RawValue!;
        }

        public void PushInheritance(Type type, string id, DeserializationResult? baseResult, HashSet<string> changed)
        {
            changed.Add(id);

            var myRes = _prototypeResults[type][id];
            var newResult = baseResult != null ? myRes.PushInheritanceFrom(baseResult) : myRes;

            foreach (var childID in _inheritanceTrees[type].Children(id))
            {
                PushInheritance(type, childID, newResult, changed);
            }

            if (newResult.RawValue is not IInheritingPrototype inheritingPrototype)
            {
                Logger.ErrorS("Serv3", $"PushInheritance was called on non-inheriting prototype! ({type}, {id})");
                return;
            }

            if (!inheritingPrototype.Abstract)
                newResult.CallAfterDeserializationHook();
            var populatedRes =
                _serializationManager.PopulateDataDefinition(_prototypes[type][id], (IDeserializedDefinition)newResult);
            _prototypes[type][id] = (IPrototype)populatedRes.RawValue!;
        }

        /// <inheritdoc />
        public List<IPrototype> LoadDirectory(ResourcePath path, bool overwrite = false)
        {
            var changedPrototypes = new List<IPrototype>();

            _hasEverBeenReloaded = true;
            var streams = Resources.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "yml" && !filePath.Filename.StartsWith("."));

            foreach (var resourcePath in streams)
            {
                var filePrototypes = LoadFile(resourcePath, overwrite);
                changedPrototypes.AddRange(filePrototypes);
            }

            return changedPrototypes;
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
                        if (!_prototypeTypes.ContainsKey(type))
                        {
                            throw new PrototypeLoadException($"Unknown prototype type: '{type}'", resourcePath.ToString(), node);
                        }

                        var mapping = node.ToDataNodeCast<MappingDataNode>();
                        mapping.Remove("type");
                        var errorNodes = _serializationManager.ValidateNode(_prototypeTypes[type], mapping).GetErrors()
                            .ToHashSet();
                        if (errorNodes.Count == 0) continue;
                        if (!dict.TryGetValue(resourcePath.ToString(), out var hashSet))
                            dict[resourcePath.ToString()] = new HashSet<ErrorNode>();
                        dict[resourcePath.ToString()].UnionWith(errorNodes);
                    }
                }
            }

            return dict;
        }

        private StreamReader? ReadFile(ResourcePath file, bool @throw = true)
        {
            var retries = 0;

            // This might be shit-code, but its pjb-responded-idk-when-asked shit-code.
            while (true)
            {
                try
                {
                    var reader = new StreamReader(Resources.ContentFileRead(file), EncodingHelpers.UTF8);
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

        public HashSet<IPrototype> LoadFile(ResourcePath file, bool overwrite = false)
        {
            var changedPrototypes = new HashSet<IPrototype>();

            try
            {
                using var reader = ReadFile(file, !overwrite);

                if (reader == null)
                {
                    return changedPrototypes;
                }

                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                LoadedData?.Invoke(yamlStream, file.ToString());

                for (var i = 0; i < yamlStream.Documents.Count; i++)
                {
                    try
                    {
                        var documentPrototypes = LoadFromDocument(yamlStream.Documents[i], overwrite, file.ToString());
                        changedPrototypes.UnionWith(documentPrototypes);
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

            return changedPrototypes;
        }

        public List<IPrototype> LoadFromStream(TextReader stream, bool overwrite = false)
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
                    var documentPrototypes = LoadFromDocument(document, overwrite);
                    changedPrototypes.AddRange(documentPrototypes);
                }
                catch (Exception e)
                {
                    throw new PrototypeLoadException($"Failed to load prototypes from document#{i}", e, "[anonymous]");
                }
            }

            LoadedData?.Invoke(yaml, "anonymous prototypes YAML stream");

            return changedPrototypes;
        }

        public List<IPrototype> LoadString(string str, bool overwrite = false)
        {
            return LoadFromStream(new StringReader(str), overwrite);
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
                        tree.RemoveId(id);
                    }

                    _prototypes[type].Remove(id);
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

        private HashSet<IPrototype> LoadFromDocument(YamlDocument document, bool overwrite = false, string? filename = null)
        {
            var changedPrototypes = new HashSet<IPrototype>();
            var rootNode = (YamlSequenceNode)document.RootNode;
            filename ??= "[anonymous]";
            string? previousProtoID = null;

            foreach (YamlMappingNode node in rootNode.Cast<YamlMappingNode>())
            {
                if (!node.TryGetNode("type", out var typeNode))
                {
                    throw new PrototypeLoadException($"Missing 'type' property", filename, node);
                }

                var type = typeNode.AsString();

                if (!_prototypeTypes.ContainsKey(type))
                {
                    throw new PrototypeLoadException($"Unknown prototype type: '{type}'", filename, node);
                }

                var dataNode = node.ToDataNodeCast<MappingDataNode>();
                BeforePrototypeLoad?.Invoke(dataNode);

                var prototypeType = _prototypeTypes[type];
                var res = _serializationManager.Read(prototypeType, dataNode, skipHook: true);
                var prototype = (IPrototype)res.RawValue!;

                if (!overwrite && _prototypes[prototypeType].ContainsKey(prototype.ID))
                {
                    throw new PrototypeLoadException($"Duplicate ID: '{prototype.ID}'", filename, node);
                }

                _prototypeResults[prototypeType][prototype.ID] = res;
                if (prototype is IInheritingPrototype inheritingPrototype)
                {
                    _inheritanceTrees[prototypeType].AddId(prototype.ID, inheritingPrototype.Parent, true);
                }
                else
                {
                    //we call it here since it wont get called when pushing inheritance
                    res.CallAfterDeserializationHook();
                }

                _prototypes[prototypeType][prototype.ID] = prototype;
                changedPrototypes.Add(prototype);

                _prototypeOrdering[prototypeType].Remove(prototype.ID);
                _prototypeExtendedData[prototypeType].Remove(prototype.ID);
                _prototypeEventDefs[prototypeType].Remove(prototype.ID);

                if (node.TryGetNode("ordering", out var orderingNode) && orderingNode is YamlMappingNode orderingMappingNode)
                {
                    ParseOrdering(previousProtoID, prototypeType, prototype, orderingMappingNode);
                }

                if (node.TryGetNode("extendedData", out var extDataNode) && extDataNode is YamlSequenceNode extDataSequenceNode)
                {
                    ParseExtendedData(filename, prototypeType, prototype, extDataSequenceNode);
                }

                if (node.TryGetNode("events", out var eventsNode) && eventsNode is YamlSequenceNode eventsSequenceNode)
                {
                    ParseEvents(filename, prototypeType, prototype, eventsSequenceNode);
                }

                previousProtoID = prototype.ID;
            }

            return changedPrototypes;
        }

        private void ParseOrdering(string? previousProtoID, Type prototypeType, IPrototype prototype, YamlMappingNode orderingMappingNode)
        {
            string[]? before = null;
            string[]? after = null;
            if (orderingMappingNode.TryGetNode("before", out var orderBeforeNode))
            {
                before = new[] { orderBeforeNode.AsString() };
            }
            if (orderingMappingNode.TryGetNode("after", out var orderAfterNode))
            {
                after = new[] { orderAfterNode.AsString() };
            }

            // Order prototypes sequentially based on their order in the document
            // if no other ordering is specified.
            if (before == null && after == null && previousProtoID != null)
                after = new[] { previousProtoID };

            var ordering = new PrototypeOrderingData(Before: before ?? Array.Empty<string>(), After: after ?? Array.Empty<string>());
            _prototypeOrdering[prototypeType][prototype.ID] = ordering;
        }

        private void ParseExtendedData(string? filename, Type prototypeType, IPrototype prototype, YamlSequenceNode extDataSequenceNode)
        {
            foreach (var child in extDataSequenceNode.Children.Cast<YamlMappingNode>())
            {
                if (!child.TryGetNode("type", out var extDataTypeNode))
                {
                    throw new PrototypeLoadException($"Extended data entry is missing 'type' property", filename, child);
                }

                var extDataTypeStr = extDataTypeNode.AsString();
                if (!_reflectionManager.TryLooseGetType(extDataTypeStr, out var extDataType))
                {
                    throw new PrototypeLoadException($"Unable to find type ending with '{extDataTypeStr}'", filename, child);
                }

                var extDataMappingNode = child.ToDataNodeCast<MappingDataNode>();
                var extDataRes = _serializationManager.Read(extDataType, extDataMappingNode, skipHook: true);
                var obj = (IPrototypeExtendedData)extDataRes.RawValue!;

                var objs = _prototypeExtendedData[prototypeType].GetValueOrInsert(prototype.ID, () => new());
                objs.Add(obj.GetType(), obj);
            }
        }

        private void ParseEvents(string filename, Type prototypeType, IPrototype prototype, YamlSequenceNode eventsSequenceNode)
        {
            var node = eventsSequenceNode.ToDataNodeCast<SequenceDataNode>();
            _prototypeEventDefs[prototypeType][prototype.ID] = _serializationManager.ReadValueOrThrow<List<PrototypeEventHandlerDef>>(node);
        }

        /// <inheritdoc />
        public List<IPrototype> LoadFromCachedResults(PrototypeManagerCache cache)
        {
            var changedPrototypes = new List<IPrototype>();

            _hasEverBeenReloaded = true;

            foreach (var (prototypeType, protos) in cache.PrototypeResults)
            {
                foreach (var (prototypeId, res) in protos)
                {
                    var prototype = (IPrototype)res.RawValue!;

                    _prototypeResults[prototypeType][prototype.ID] = res;
                    if (prototype is IInheritingPrototype inheritingPrototype)
                    {
                        _inheritanceTrees[prototypeType].AddId(prototype.ID, inheritingPrototype.Parent, true);
                    }
                    else
                    {
                        //we call it here since it wont get called when pushing inheritance
                        res.CallAfterDeserializationHook();
                    }

                    _prototypes[prototypeType][prototype.ID] = prototype;
                    changedPrototypes.Add(prototype);
                }
            }

            foreach (var (prototypeType, orderings) in cache.PrototypeOrdering)
            {
                _prototypeOrdering[prototypeType].Clear();
                foreach (var (prototypeId, orderingData) in orderings)
                {
                    _prototypeOrdering[prototypeType][prototypeId] = orderingData;
                }
            }

            foreach (var (prototypeType, extendedDatas) in cache.PrototypeExtendedData)
            {
                _prototypeExtendedData[prototypeType].Clear();
                foreach (var (prototypeId, objs) in extendedDatas)
                {
                    _prototypeExtendedData[prototypeType][prototypeId] = objs.ShallowClone();
                }
            }

            foreach (var (prototypeType, eventDefLists) in cache.PrototypeEventDefs)
            {
                _prototypeEventDefs[prototypeType].Clear();
                foreach (var (prototypeId, eventDefs) in eventDefLists)
                {
                    _prototypeEventDefs[prototypeType][prototypeId] = eventDefs.ToList();
                }
            }

            return changedPrototypes;
        }

        public bool HasIndex<T>(PrototypeId<T> id) where T : class, IPrototype
            => HasIndex(typeof(T), (string)id);

        public bool HasIndex(Type type, string id)
        {
            if (!_prototypes.TryGetValue(type, out var index))
            {
                throw new UnknownPrototypeException(id, type);
            }

            return index.ContainsKey(id);
        }

        public bool TryIndex<T>(PrototypeId<T> id, [NotNullWhen(true)] out T? prototype) where T : class, IPrototype
        {
            var returned = TryIndex(typeof(T), (string)id, out var proto);
            prototype = (proto ?? null) as T;
            return returned;
        }

        public bool TryIndex(Type type, string id, [NotNullWhen(true)] out IPrototype? prototype)
        {
            if (!_prototypes.TryGetValue(type, out var index))
            {
                throw new UnknownPrototypeException(id, type);
            }

            return index.TryGetValue(id, out prototype);
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
            where TExt : class, IPrototypeExtendedData
        {
            if (!TryGetExtendedData<TProto, TExt>(id, out var data))
                throw new KeyNotFoundException($"Extended data {typeof(TExt)} for {typeof(TProto)}:{id} not found.");

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
            where TExt : class, IPrototypeExtendedData
        {
            return HasExtendedData(typeof(TProto), typeof(TExt), (string)id);
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
            where TExt : class, IPrototypeExtendedData
        {
            if (!TryGetExtendedData(typeof(TProto), typeof(TExt), (string)id, out var obj))
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

            _prototypeTypes[attribute.Type] = type;
            _prototypePriorities[type] = attribute.LoadPriority;

            if (typeof(IPrototype).IsAssignableFrom(type))
            {
                _prototypes[type] = new Dictionary<string, IPrototype>();
                _prototypeResults[type] = new Dictionary<string, DeserializationResult>();
                if (typeof(IInheritingPrototype).IsAssignableFrom(type))
                    _inheritanceTrees[type] = new PrototypeInheritanceTree();
                _prototypeOrdering[type] = new Dictionary<string, PrototypeOrderingData>();
                _prototypeExtendedData[type] = new Dictionary<string, Dictionary<Type, IPrototypeExtendedData>>();
                _prototypeEventDefs[type] = new Dictionary<string, List<PrototypeEventHandlerDef>>();
            }
        }

        public event Action<YamlStream, string>? LoadedData;
        public event Action<PrototypesReloadedEventArgs>? PrototypesReloaded;
        public event Action<MappingDataNode>? BeforePrototypeLoad;
    }

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
        public override string Message => "Unknown prototype: " + Prototype;
        public readonly string? Prototype;
        public readonly Type? Type;

        public UnknownPrototypeException(string? prototype, Type? type)
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

    public sealed record PrototypesReloadedEventArgs(IReadOnlyDictionary<Type, PrototypesReloadedEventArgs.PrototypeChangeSet> ByType)
    {
        public sealed record PrototypeChangeSet(IReadOnlyDictionary<string, IPrototype> Modified);
    }
}

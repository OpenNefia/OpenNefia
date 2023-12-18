using OpenNefia.Core.Areas;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using System.Runtime.Serialization;
using ICSharpCode.Decompiler.CSharp.OutputVisitor;
using Love;
using static System.Runtime.InteropServices.JavaScript.JSType;
using EngineVariableId = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.EngineVariables.EngineVariablePrototype>;

namespace OpenNefia.Core.EngineVariables
{
    /// <summary>
    /// Manages the global engine config.
    /// It's a way of describing previously hard-coded constants in an external config format so they can be easily hot-reloaded. 
    /// Mods can also override these variables declaratively or programatically if they wish.
    /// </summary>
    /// <remarks>
    /// A future design:
    /// Mods have a "Variables" folder. They can override variables previously defined by specifying the same key as another variable (so long as the types match).
    /// If the variable is a list or other complex data structure, certain classes can be specified as the key value for data transformations on the variable.
    /// </remarks>
    public interface IEngineVariablesManager
    {
        void LoadFromStream(TextReader stream);
        void LoadDirectory(ResourcePath path);
        void LoadString(string str);
        void LoadFile(ResourcePath file);

        T Get<T>(EngineVariableId id);
        void Set(EngineVariableId id, DataNode value);
    }

    internal interface IEngineVariablesManagerInternal : IEngineVariablesManager
    {
        void Initialize();

        void WatchResources();
    }

    public sealed partial class EngineVariablesManager : IEngineVariablesManagerInternal
    {
        [Dependency] private readonly IEntityManagerInternal _entityManager = default!;
        [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IAreaManagerInternal _areaManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSessionManager = default!;
        [Dependency] private readonly IModLoader _modLoader = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        public const string SawmillName = "enginevars";

        private class EngineVariableRegistration
        {
            public AbstractFieldInfo FieldInfo { get; }
            public object Parent { get; }

            public Type Type => FieldInfo.FieldType;

            public EngineVariableRegistration(AbstractFieldInfo propertyInfo, object parent)
            {
                FieldInfo = propertyInfo;
                Parent = parent;
            }

            public void SetValueOnParent(object value)
            {
                if (FieldInfo.TryGetBackingField(out var backingField))
                {
                    backingField.SetValue(Parent, value);
                }
                else
                {
                    FieldInfo.SetValue(Parent, value);
                }
            }
        }

        private record class EngineVariableDef(EngineVariablePrototype proto);

        private Dictionary<string, EngineVariableDef> _varDefs = new();
        private Dictionary<string, List<EngineVariableRegistration>> _varRegistrations = new();
        private Dictionary<string, DataNode> _varValues = new();

        public void Initialize()
        {
            LoadVariableDefinitions();

            foreach (var type in _entitySystemManager.SystemTypes)
            {
                if (_entitySystemManager.TryGetEntitySystem(type, out var sys))
                {
                    RegisterEngineVariablesFromFields(sys);
                }
            }

            foreach (var instance in IoCManager.Instance!.Services.Values)
            {
                RegisterEngineVariablesFromFields(instance);
            }

            _graphics.OnWindowFocusChanged += WindowFocusedChanged;

            WatchResources();
        }

        private void LoadVariableDefinitions()
        {
            if (_varDefs.Count > 0)
                throw new InvalidOperationException("Engine variables were already loaded.");

            foreach (var proto in _prototypeManager.EnumeratePrototypes<EngineVariablePrototype>())
            {
                var id = (string)proto.GetStrongID();
                _varDefs.Add(id, new EngineVariableDef(proto));
                _varValues.Add(id, proto.Default.Node.Copy());
            }
        }

        private void RegisterEngineVariablesFromFields(object instance)
        {
            foreach (var info in instance.GetType().GetAllPropertiesAndFields())
            {
                if (info.TryGetAttribute(out EngineVariableAttribute? attr))
                {
                    var value = info.GetValue(instance);
                    // if (value == null)
                    //    throw new InvalidDataException($"Expected non-nullable reference for save data '{attr.Key}', got null.");

                    RegisterEngineVariable(attr.Key, info, instance);
                }
            }
        }

        private void RegisterEngineVariable(string key, AbstractFieldInfo field, object parent)
        {
            var ty = field.FieldType;

            if (field is SpecificPropertyInfo propertyInfo)
            {
                if (propertyInfo.PropertyInfo.GetMethod == null)
                {
                    throw new ArgumentException($"Property {propertyInfo} is annotated with {nameof(EngineVariableAttribute)} but has no getter");
                }
                else if (propertyInfo.PropertyInfo.SetMethod == null)
                {
                    if (!propertyInfo.HasBackingField())
                    {
                        throw new ArgumentException($"Property {propertyInfo} in type {propertyInfo.DeclaringType} is annotated with {nameof(EngineVariableAttribute)} as non-readonly but has no auto-setter");
                    }
                }
            }
            if (!_varDefs.TryGetValue(key, out var def))
            {
                throw new ArgumentException($"Engine variable '{key}' not defined (type: {ty})", nameof(key));
            }
            if (!_varRegistrations.TryGetValue(key, out var regs))
            {
                _varRegistrations.Add(key, new List<EngineVariableRegistration>());
                regs = _varRegistrations[key];
            }
            if (!_serializationManager.CanSerializeType(ty))
            {
                throw new ArgumentException($"Type '{ty}' cannot be serialized.", nameof(ty));
            }

            Logger.DebugS(SawmillName, $"Registering engine variable: {key} ({ty})");
            var reg = new EngineVariableRegistration(field, parent);
            regs.Add(reg);

            var node = _varValues[key];
            var data = _serializationManager.Read(ty, node)!;
            reg.SetValueOnParent(data);
        }

        public T Get<T>(EngineVariableId id)
        {
            if (!_varValues.TryGetValue((string)id, out var value))
            {
                throw new InvalidDataException($"Unknown engine variable: {id}");
            }

            return _serializationManager.Read<T>(value);
        }

        protected void ReloadVariables(IEnumerable<ResourcePath> filePaths)
        {
#if !FULL_RELEASE
            var changed = new Dictionary<Type, HashSet<string>>();
            foreach (var filePath in filePaths)
            {
                LoadFile(filePath.ToRootedPath());
            }
#endif
        }

        public void LoadDirectory(ResourcePath path)
        {
            var streams = _resources.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "yml" && !filePath.Filename.StartsWith("."));

            foreach (var resourcePath in streams)
            {
                LoadFile(resourcePath.ToRootedPath());
            }
        }

        public void LoadFromStream(TextReader stream)
        {
            var yaml = new YamlStream();
            yaml.Load(stream);

            for (var i = 0; i < yaml.Documents.Count; i++)
            {
                var document = yaml.Documents[i];
                try
                {
                    LoadFromDocument(document);
                }
                catch (Exception e)
                {
                    throw new PrototypeLoadException($"Failed to load engine variables from document#{i}", e, "[anonymous]");
                }
            }

            // LoadedData?.Invoke(yaml, "anonymous engine variables YAML stream");
        }

        public void LoadString(string str)
        {
            LoadFromStream(new StringReader(str));
        }

        public void LoadFile(ResourcePath file)
        {
            try
            {
                using var reader = ReadFile(file, @throw: true);

                if (reader == null)
                {
                    return;
                }

                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                // LoadedData?.Invoke(yamlStream, file.ToString());

                for (var i = 0; i < yamlStream.Documents.Count; i++)
                {
                    try
                    {
                        LoadFromDocument(yamlStream.Documents[i], file.ToString());
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorS(SawmillName, $"Exception whilst loading engine variables from {file}#{i}:\n{e}");
                    }
                }
            }
            catch (YamlException e)
            {
                var sawmill = Logger.GetSawmill(SawmillName);
                sawmill.Error("YamlException whilst loading engine variables from {0}: {1}", file, e.Message);
            }
        }

        private void LoadFromDocument(YamlDocument document, string? filename = null)
        {
            var rootNode = (YamlMappingNode)document.RootNode;
            filename ??= "[anonymous]";

            foreach (var entry in rootNode.Children)
            {
                var ns = entry.Key.ToDataNodeCast<ValueDataNode>();
                var values = entry.Value.ToDataNodeCast<MappingDataNode>();

                foreach (var valueEntry in values.Children)
                {
                    var key = (ValueDataNode)valueEntry.Key;
                    var value = valueEntry.Value;

                    var id = new EngineVariableId($"{ns.Value}.{key}");

                    try
                    {
                        Set(id, value);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorS(SawmillName, $"Exception whilst loading engine variable '{id}' from {filename}:\n{e}");
                    }

                    Logger.InfoS(SawmillName, $"Loaded engine variable override: {id}");
                    Logger.DebugS(SawmillName, $"{value}");
                }
            }
        }

        public void Set(EngineVariableId id, DataNode value)
        {
            var id_ = (string)id;
            if (!_varDefs.ContainsKey(id_))
            {
                throw new InvalidDataException($"Unknown engine variable: {id}");
            }

            _varValues[id_] = value;

            Logger.DebugS(SawmillName, $"Set engine variable: {id} - {value}");

            // TODO better error handling
            if (_varRegistrations.TryGetValue(id_, out var regs))
            {
                foreach (var reg in regs)
                {
                    try
                    {
                        var data = _serializationManager.Read(reg.Type, value)!;
                        reg.SetValueOnParent(data);
                    }
                    catch (Exception ex) 
                    {
                        Logger.ErrorS(SawmillName, ex, $"Failed to set engine variable property: {id} - {value}");
                    }
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
                    var reader = new StringReader(_resources.ContentFileReadAllText(file, EncodingHelpers.UTF8));
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

                        Logger.ErrorS(SawmillName, $"Error reloading engine variables in file {file}.", e);
                        return null;
                    }

                    retries++;
                    Thread.Sleep(10);
                }
            }
        }
    }

    [Serializable]
    public class EngineVariableLoadException : Exception
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

        public EngineVariableLoadException()
        {
        }

        public EngineVariableLoadException(string message, string? filename = null, YamlNode? node = null) : base(message)
        {
            Filename = filename;
            Node = node;
        }

        public EngineVariableLoadException(string message, Exception inner, string? filename = null) : base(message, inner)
        {
            Filename = filename;
        }

        public EngineVariableLoadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("filename", Filename, typeof(string));
            info.AddValue("node", Node, typeof(YamlNode));
        }
    }
}

using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Prototypes
{
    /// <summary>
    /// A manager for applying modifications to prototype YAML
    /// before they're loaded by the prototype manager. The list of
    /// modifications are defined in an <see cref="ITheme"/>
    /// and matched against each prototype's YAML individually.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is separate from the prototype system since themes need
    /// to be loaded before any prototypes are loaded.
    /// </para>
    /// <para>
    /// The intention is for this to eventually serve the same purpose as
    /// RimWorld's PatchOperations, allowing for more types of patching logic
    /// than simple merging.
    /// (https://rimworldwiki.com/wiki/Modding_Tutorials/PatchOperations)
    /// </para>
    /// </remarks>
    public interface IThemeManager
    {
        IReadOnlyList<ITheme> ActiveThemes { get; }

        /// <summary>
        /// Returns an IEnumerable to iterate all themes.
        /// </summary>
        IEnumerable<ITheme> EnumerateThemes();

        void Initialize();

        void Shutdown();
        
        /// <summary>
        /// Index for an <see cref="ITheme"/> by ID.
        /// </summary>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the theme is not registered.
        /// </exception>
        ITheme Index(string id);

        /// <summary>
        ///     Returns whether a theme with the specified <param name="id"/> exists.
        /// </summary>
        bool HasIndex(string id);
        bool TryIndex(string id, [NotNullWhen(true)] out ITheme? theme);

        /// <summary>
        /// Clears out all themes and reset to a blank slate.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the active theme.
        /// </summary>
        /// <remarks>
        /// NOTE: This needs to be called before loading any prototypes.
        /// </remarks>
        void SetActiveTheme(string id);

        /// <summary>
        /// Load themes from files in a directory, recursively.
        /// </summary>
        List<ITheme> LoadDirectory(ResourcePath path, bool overwrite = false);

        List<ITheme> LoadFromStream(TextReader stream, bool overwrite = false);

        List<ITheme> LoadString(string str, bool overwrite = false);
    }

    public class ThemeManager : IThemeManager
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IResourceManager _resources = default!;
        [Dependency] private readonly ISerializationManager _serializationManager = default!;

        private readonly Dictionary<string, ITheme> _themes = new();
        private readonly List<ITheme> _activeThemes = new();

        // { themeid -> { type -> { id -> override YAML } } }
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, MappingDataNode>>> _allOverrides = new();

        // ( type, id )
        private readonly HashSet<StructMultiKey<string, string>> _affectedPrototypes = new();

        public IReadOnlyList<ITheme> ActiveThemes => _activeThemes;

        private bool _initialized;
        private bool _hasEverBeenReloaded;

        public void Initialize()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"{nameof(ThemeManager)} has already been initialized.");
            }

            _initialized = true;

            _prototypeManager.BeforePrototypeLoad += HandleBeforePrototypeLoad;
        }

        public void Shutdown()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException($"{nameof(ThemeManager)} has not been initialized.");
            }

            _prototypeManager.BeforePrototypeLoad -= HandleBeforePrototypeLoad;
        }

        private void HandleBeforePrototypeLoad(MappingDataNode prototypeYaml)
        {
            var type = ((ValueDataNode)prototypeYaml.Get("type")).Value;
            var id = ((ValueDataNode)prototypeYaml.Get("id")).Value;

            foreach (var theme in _activeThemes)
            {
                var forTheme = _allOverrides[theme.ID];

                if (!forTheme.TryGetValue(type, out var byId))
                    return;

                if (byId.TryGetValue(id, out var overrideNode))
                {
                    DeepMerge(overrideNode, prototypeYaml);
                    _affectedPrototypes.Add(new(type, id));
                }
            }
        }

        private static void DeepMerge(MappingDataNode from, MappingDataNode into)
        {
            foreach (var (key, fromNode) in from.Children)
            {
                if (!into.TryGet(key, out var intoNode))
                    throw new KeyNotFoundException($"Could not find key '{key}' for merging in target node.");

                switch (fromNode)
                {
                    case MappingDataNode fromMapping:
                        if (intoNode is not MappingDataNode intoMapping)
                        {
                            throw new InvalidNodeTypeException(
                                $"Cannot deep merge from {intoNode.GetType()}. Expected {nameof(MappingDataNode)}");
                        }
                        DeepMerge(fromMapping, intoMapping);
                        break;
                    case SequenceDataNode fromSeq:
                        if (intoNode is not SequenceDataNode intoSeq)
                        {
                            throw new InvalidNodeTypeException(
                                $"Cannot deep merge from {intoNode.GetType()}. Expected {nameof(SequenceDataNode)}");
                        }
                        intoSeq.Clear();
                        foreach (var node in fromSeq.Sequence)
                        {
                            intoSeq.Add(node.Copy());
                        }
                        break;
                    case ValueDataNode fromValue:
                        if (intoNode is not ValueDataNode intoValue)
                        {
                            throw new InvalidNodeTypeException(
                                $"Cannot deep merge from {intoNode.GetType()}. Expected {nameof(SequenceDataNode)}");
                        }
                        intoValue.Value = fromValue.Value;
                        break;
                    default:
                        throw new InvalidOperationException($"Could not deep merge node of type '{fromNode.GetType()}'.");
                }
            }
        }

        public void SetActiveTheme(string id)
        {
            if (!TryIndex(id, out var theme))
                throw new KeyNotFoundException($"Theme with ID '{id}' not found.");

            _activeThemes.Clear();
            _activeThemes.Add(theme);
        }

        public void Clear()
        {
            _activeThemes.Clear();
            _themes.Clear();
            _allOverrides.Clear();
            _affectedPrototypes.Clear();
        }

        public IEnumerable<ITheme> EnumerateThemes()
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No themes have been loaded yet.");
            }

            return _themes.Values;
        }

        public ITheme Index(string id)
        {
            if (!_hasEverBeenReloaded)
            {
                throw new InvalidOperationException("No themes have been loaded yet.");
            }

            return _themes[(string)id];
        }

        public bool HasIndex(string id)
        {
            return _themes.ContainsKey(id);
        }

        public bool TryIndex(string id, [NotNullWhen(true)] out ITheme? theme)
        {
            return _themes.TryGetValue(id, out theme);
        }

        private HashSet<ITheme> LoadFromDocument(YamlDocument document, bool overwrite = false, string? filename = null)
        {
            var changedThemes = new HashSet<ITheme>();
            var rootNode = (YamlSequenceNode)document.RootNode;
            filename ??= "[anonymous]";

            foreach (YamlMappingNode node in rootNode.Cast<YamlMappingNode>())
            {
                var res = _serializationManager.Read(typeof(Theme), node.ToDataNode(), skipHook: true);
                var theme = (ITheme)res.RawValue!;

                if (!overwrite && _themes.ContainsKey(theme.ID))
                {
                    throw new PrototypeLoadException($"Duplicate ID: '{theme.ID}'", filename, node);
                }

                res.CallAfterDeserializationHook();

                var forTheme = _allOverrides.GetOrNew(theme.ID);

                foreach (var entry in theme.Overrides.Cast<MappingDataNode>())
                {
                    if (!entry.TryGet("type", out var typeNode))
                    {
                        throw new PrototypeLoadException($"Missing 'type' property", filename, node);
                    }

                    var type = ((ValueDataNode)typeNode).Value;
                    
                    if (!entry.TryGet("id", out var idNode))
                    {
                        throw new PrototypeLoadException($"Missing 'id' property", filename, node);
                    }

                    var id = ((ValueDataNode)idNode).Value;

                    forTheme.GetOrNew(type).Add(id, entry);
                }

                _themes[theme.ID] = theme;
                changedThemes.Add(theme);
            }

            return changedThemes;
        }

        public List<ITheme> LoadDirectory(ResourcePath path, bool overwrite = false)
        {
            var changedThemes = new List<ITheme>();

            _hasEverBeenReloaded = true;
            var streams = _resources.ContentFindFiles(path).ToList().AsParallel()
                .Where(filePath => filePath.Extension == "yml" && !filePath.Filename.StartsWith("."));

            foreach (var resourcePath in streams)
            {
                var fileThemes = LoadFile(resourcePath, overwrite);
                changedThemes.AddRange(fileThemes);
            }

            return changedThemes;
        }

        private StreamReader? ReadFile(ResourcePath file, bool @throw = true)
        {
            var retries = 0;

            // This might be shit-code, but its pjb-responded-idk-when-asked shit-code.
            while (true)
            {
                try
                {
                    var reader = new StreamReader(_resources.ContentFileRead(file), EncodingHelpers.UTF8);
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

                        Logger.ErrorS("theme", $"Error reloading themes in file {file}.", e);
                        return null;
                    }

                    retries++;
                    Thread.Sleep(10);
                }
            }
        }

        public HashSet<ITheme> LoadFile(ResourcePath file, bool overwrite = false)
        {
            var changedThemes = new HashSet<ITheme>();

            try
            {
                using var reader = ReadFile(file, !overwrite);

                if (reader == null)
                {
                    return changedThemes;
                }

                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                for (var i = 0; i < yamlStream.Documents.Count; i++)
                {
                    try
                    {
                        var documentThemes = LoadFromDocument(yamlStream.Documents[i], overwrite, file.ToString());
                        changedThemes.UnionWith(documentThemes);
                    }
                    catch (Exception e)
                    {
                        Logger.ErrorS("theme", $"Exception whilst loading themes from {file}#{i}:\n{e}");
                    }
                }
            }
            catch (YamlException e)
            {
                Logger.ErrorS("theme", e, "YamlException whilst loading themes from {0}", file);
            }

            return changedThemes;
        }

        public List<ITheme> LoadFromStream(TextReader stream, bool overwrite = false)
        {
            var changedThemes = new List<ITheme>();
            _hasEverBeenReloaded = true;
            var yaml = new YamlStream();
            yaml.Load(stream);

            for (var i = 0; i < yaml.Documents.Count; i++)
            {
                var document = yaml.Documents[i];
                try
                {
                    var documentThemes = LoadFromDocument(document, overwrite);
                    changedThemes.AddRange(documentThemes);
                }
                catch (Exception e)
                {
                    throw new PrototypeLoadException($"Failed to load themes from document#{i}", e, "[anonymous]");
                }
            }

            return changedThemes;
        }

        public List<ITheme> LoadString(string str, bool overwrite = false)
        {
            return LoadFromStream(new StringReader(str), overwrite);
        }
    }
}

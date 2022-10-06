using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.ExceptionServices;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Serialization.Manager;
using YamlDotNet.RepresentationModel;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.Markdown.Sequence;

namespace OpenNefia.Core.ContentPack
{
    /// <summary>
    ///     Class for managing the loading of assemblies into the engine.
    /// </summary>
    internal sealed class ModLoader : BaseModLoader, IModLoaderInternal, IDisposable
    {
        [Dependency] private readonly IResourceManagerInternal _res = default!;
        [Dependency] private readonly ILogManager _logManager = default!;

        private static readonly ResourcePath ModManifestPath = new("/About/Mod.yml");

        // List of extra assemblies side-loaded from the /Assemblies/ mounted path.
        private readonly List<Assembly> _sideModules = new();

        private readonly AssemblyLoadContext _loadContext;

        private readonly object _lock = new();

        private static int _modLoaderId;

        private bool _useLoadContext = true;

        private readonly List<ExtraModuleLoad> _extraModuleLoads = new();

        public event ExtraModuleLoad ExtraModuleLoaders
        {
            add => _extraModuleLoads.Add(value);
            remove => _extraModuleLoads.Remove(value);
        }

        public ModLoader()
        {
            var id = Interlocked.Increment(ref _modLoaderId);
            // Imma just turn on collectible assemblies for the heck of it.
            // Even though we don't need it yet.
            _loadContext = new AssemblyLoadContext($"ModLoader-{id}", true);

            _loadContext.Resolving += ResolvingAssembly;

            AssemblyLoadContext.Default.Resolving += DefaultOnResolving;
        }

        public void SetUseLoadContext(bool useLoadContext)
        {
            _useLoadContext = useLoadContext;
            Logger.DebugS("res", "{0} assembly load context", useLoadContext ? "ENABLING" : "DISABLING");
        }

        public Func<string, Stream?>? VerifierExtraLoadHandler { get; set; }

        /// <summary>
        /// Serialization can only be initialized after mods have been loaded, so the manual
        /// deserialization is necessary.
        /// </summary>
        private ModManifest LoadModManifest(ContentRootID rootID, YamlStream yaml)
        {
            var rootNode = yaml.Documents[0].RootNode.ToDataNodeCast<MappingDataNode>();
            var modNode = (MappingDataNode)rootNode["mod"];

            if (!modNode.TryGet<ValueDataNode>("id", out var idNode))
                throw new InvalidDataException("Missing 'id' node in mod manifest");
            var id = idNode.Value;

            if (!modNode.TryGet<ValueDataNode>("version", out var versionNode))
                throw new InvalidDataException("Missing 'version' node in mod manifest");
            if (!Version.TryParse(versionNode.Value, out var version))
                throw new InvalidDataException($"Version '{versionNode.Value}' is invalid.");

            var dependencies = new List<ModDependency>();
            if (modNode.TryGet<SequenceDataNode>("dependencies", out var dependenciesNode))
            {
                foreach (var dependencyNode in dependenciesNode.Cast<MappingDataNode>())
                {
                    if (!dependencyNode.TryGet<ValueDataNode>("id", out var depIdNode))
                        throw new InvalidDataException("Missing 'id' node in mod dependency");
                    dependencies.Add(new ModDependency(depIdNode.Value));
                }
            }

            return new ModManifest(rootID, id, version, dependencies);
        }

        public bool TryLoadModulesFrom(ResourcePath mountPath, string filterPrefix)
        {
            var sw = Stopwatch.StartNew();
            Logger.DebugS("res.mod", "LOADING modules");
            var files = new Dictionary<string, (ModManifest Manifest, ResourcePath? MainAssemblyPath, string[] Dependencies)>();

            foreach (var (rootID, root) in _res.GetContentRootsAndIDs())
            {
                // Get the Mod.yml at the root of this content directory.
                if (_res.ContentFileExists(ModManifestPath, rootID))
                {
                    var yamlStream = _res.ContentFileReadYaml(ModManifestPath, rootID);
                    var manifest = LoadModManifest(rootID, yamlStream);
                    Logger.InfoS("res.mod", $"Loaded manifest for mod '{manifest.ID}'");

                    // Get only the assemblies that are under this content root so they can be
                    // associated with the correct mod manifest.
                    var assemblies = _res.ContentFindRelativeFiles(mountPath, rootID)
                                         .Where(p => !p.ToString().Contains('/') 
                                            && p.Filename.StartsWith(filterPrefix) 
                                            && p.Extension == "dll")
                                         .ToList();

                    // There should be a single "main assembly" that acts as the code for this mod.
                    // Extra assemblies needed by the main assembly will also be sideloaded (eventually)
                    var mainAssemblyPaths = assemblies.Where(a => a.FilenameWithoutExtension == manifest.ID).ToList();

                    // If there are no assemblies to load, this is a non-code mod
                    // (manifest and prototypes/locale data only)
                    ResourcePath? mainAssemblyPath = null;

                    if (mainAssemblyPaths.Count > 0)
                    {
                        mainAssemblyPath = mountPath / mainAssemblyPaths.First();

                        // TODO implement extra assembly loading

                        Logger.Info("res.mod", $"Load assembly for module {manifest.ID}: {mainAssemblyPath}");
                    }

                    // TODO handle versions/constraints
                    var deps = manifest.Dependencies.Select(d => d.ID).ToArray();

                    if (!files.TryAdd(manifest.ID, (manifest, mainAssemblyPath, deps)))
                    {
                        Logger.ErrorS("res.mod", "Found multiple modules with the same ID " +
                                                 $"'{manifest.ID}', A: {files[manifest.ID].MainAssemblyPath}, B: {mainAssemblyPath}.");
                        return false;
                    }
                }
            }

            var nodes = TopologicalSort.FromBeforeAfter(
                files,
                kv => kv.Key,
                kv => kv.Value,
                _ => Array.Empty<string>(),
                kv => kv.Value.Dependencies,
                allowMissing: true); // missing refs would be non-content assemblies so allow that.

            // Actually load them in the order they depend on each other.
            foreach (var (manifest, asmPath, _) in TopologicalSort.Sort(nodes))
            {
                Logger.DebugS("res.mod", $"Loading module: {manifest.ID} (assembly: {asmPath})");
                try
                {
                    if (asmPath == null)
                    {
                        InitMod(manifest);
                    }
                    else
                    {
                        // If possible, load from disk path instead.
                        // This probably improves performance or something and makes debugging etc more reliable.
                        if (_res.TryGetDiskFilePath(asmPath, out var diskPath, manifest.ContentRootID))
                        {
                            LoadGameAssembly(manifest, diskPath, skipVerify: true);
                        }
                        else
                        {
                            using var assemblyStream = _res.ContentFileRead(asmPath, manifest.ContentRootID);
                            using var symbolsStream = _res.ContentFileReadOrNull(asmPath.WithExtension("pdb"), manifest.ContentRootID);
                            LoadGameAssembly(manifest, assemblyStream, symbolsStream, skipVerify: true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.ErrorS("srv", $"Exception loading module '{manifest.ID}' ({asmPath}):\n{e.ToStringWithLoaderExceptions()}");
                    return false;
                }
            }
            Logger.DebugS("res.mod", $"DONE loading modules: {sw.Elapsed}");

            return true;
        }

        public void LoadGameAssembly(ModManifest manifest, Stream assembly, Stream? symbols = null, bool skipVerify = false)
        {
            assembly.Position = 0;

            Assembly gameAssembly;
            if (_useLoadContext)
            {
                gameAssembly = _loadContext.LoadFromStream(assembly, symbols);
            }
            else
            {
                gameAssembly = Assembly.Load(assembly.CopyToArray(), symbols?.CopyToArray());
            }

            InitMod(manifest, gameAssembly);
        }

        public void LoadGameAssembly(ModManifest manifest, string diskPath, bool skipVerify = false)
        {
            Assembly assembly;
            if (_useLoadContext)
            {
                assembly = _loadContext.LoadFromAssemblyPath(diskPath);
            }
            else
            {
                assembly = Assembly.LoadFrom(diskPath);
            }

            InitMod(manifest, assembly);
        }

        private Assembly? ResolvingAssembly(AssemblyLoadContext context, AssemblyName name)
        {
            try
            {
                lock (_lock)
                {
                    _logManager.GetSawmill("res").Debug("ResolvingAssembly {0}", name);

                    // Try main modules.
                    foreach (var assembly in LoadedModAssemblies)
                    {
                        if (assembly.FullName == name.FullName)
                        {
                            return assembly;
                        }
                    }

                    if (TryLoadExtra(name) is { } asm)
                        return asm;

                    foreach (var assembly in _sideModules)
                    {
                        if (assembly.FullName == name.FullName)
                        {
                            return assembly;
                        }
                    }

                    return null;
                }
            }
            catch (Exception e)
            {
                _logManager.GetSawmill("res").Error("Exception in ResolvingAssembly: {0}", e);
                ExceptionDispatchInfo.Capture(e).Throw();
                throw null; // Unreachable.
            }
        }

        public void Dispose()
        {
            _loadContext.Unload();
            AssemblyLoadContext.Default.Resolving += DefaultOnResolving;
        }

        private Assembly? DefaultOnResolving(AssemblyLoadContext ctx, AssemblyName name)
        {
            // We have to hook AssemblyLoadContext.Default.Resolving so that C# interactive loads assemblies correctly.
            // Otherwise it would load the assemblies a second time which is an amazing way to have everything break.
            if (_useLoadContext)
            {
                _logManager.GetSawmill("res.mod").Debug($"RESOLVING DEFAULT: {name}");
                foreach (var module in LoadedModAssemblies)
                {
                    if (module.GetName().Name == name.Name)
                    {
                        return module;
                    }
                }

                foreach (var module in _sideModules)
                {
                    if (module.GetName().Name == name.Name)
                    {
                        return module;
                    }
                }

                if (TryLoadExtra(name) is { } asm)
                    return asm;
            }

            return null;
        }

        private Assembly? TryLoadExtra(AssemblyName name)
        {
            foreach (var extra in _extraModuleLoads)
            {
                if (extra(name) is { } asm)
                    return asm;
            }

            return null;
        }
    }
}

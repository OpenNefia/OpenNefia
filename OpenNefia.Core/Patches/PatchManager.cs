using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.Patches
{
    /*
     * TODO rework this system.
     * I just want to apply themesets early.
     * Maybe patches should live separately from themes to allow previewing themes via the options.
     */
    public sealed class PatchManager : IPatchManagerInternal
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;

        private readonly List<PatchInstance> _loadedPatches = new();
        private static readonly ResourcePath PatchPath = new("/Patches/");

        public void Initialize()
        {
            _protos.BeforePrototypeLoad += HandleBeforePrototypeLoad;

            ReloadPatches();
        }
        
        public void Shutdown()
        {
            _protos.BeforePrototypeLoad -= HandleBeforePrototypeLoad;
        }

        private void ReloadPatches()
        {
            _loadedPatches.Clear();

            foreach (var file in _resourceCache.ContentFindFiles(PatchPath).Where(p => p.Extension == "yml"))
            {
                var yaml = _resourceCache.ContentFileReadYaml(file);
                var yamlDoc = yaml.Documents[0].RootNode.ToDataNode();
                var patches = _serialization.Read<List<Patch>>(yamlDoc);

                foreach (var patch in patches)
                {
                    Logger.Info($"Loading patch {patch.ID}.");
                    foreach (var op in patch.Operations)
                    {
                        IoCManager.InjectDependencies(op); // TODO serializer auto-injection
                        op.Initialize();
                    }
                    _loadedPatches.Add(new PatchInstance(patch, enabled: true));
                }
            }

            Logger.InfoS("patch", $"Loaded {_loadedPatches} patches.");
            
            // TODO order patches.
        }

        private void HandleBeforePrototypeLoad(Type prototypeType, string prototypeTypeName, string prototypeID, MappingDataNode prototypeYaml)
        {
            foreach (var patch in _loadedPatches)
            {
                if (patch.Patch.Type == PatchType.Prototype && patch.Enabled)
                {
                    foreach (var op in patch.Patch.Operations)
                    {
                        op.Apply(prototypeType, prototypeTypeName, prototypeID, prototypeYaml);
                    }
                }
            }
        }
    }

    internal sealed class PatchInstance
    {
        public PatchInstance(Patch patch, bool enabled)
        {
            Patch = patch;
            Enabled = enabled;
        }

        public Patch Patch { get; }
        public bool Enabled { get; set; }
    }

    [DataDefinition]
    public sealed class Patch
    {
        [DataField(required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public PatchType Type { get; }

        [DataField(required: true)]
        public PatchApplyMode Apply { get; }

        [DataField(required: true)]
        public List<IPatchOperation> Operations { get; } = default!;
    }

    public enum PatchType
    {
        Prototype,
        Map,
        Config
    }

    public enum PatchApplyMode
    {
        AlwaysActive,
        Togglable,
        Theme
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IPatchOperation
    {
        void Initialize() { }
        void Apply(Type prototypeType, string prototypeTypeName, string prototypeID, MappingDataNode yaml);
    }
}
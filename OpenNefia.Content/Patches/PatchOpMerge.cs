using OpenNefia.Content.Portraits;
using OpenNefia.Core.Audio;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Patches;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Patches
{
    public sealed class PatchOpMerge : IPatchOperation
    {
        [Dependency] private readonly IResourceManager _resources = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        [DataField(required: true)]
        public List<MergeEdit> Edits { get; } = default!;

        private Dictionary<string, Dictionary<string, MappingDataNode>> _lookup = new();

        public void Initialize()
        {
            foreach (var edit in Edits)
            {
                var inner = _lookup.GetOrInsertNew(edit.Type);
                foreach (var innerEdit in edit.Edits)
                {
                    var id = innerEdit.Cast<ValueDataNode>("id");
                    innerEdit.Remove("id");
                    inner.Add(id.Value, innerEdit);
                }
            }
        }

        public void Apply(Type prototypeType, string prototypeTypeName, string prototypeID, MappingDataNode yaml)
        {
            if (_lookup.TryGetValue(prototypeTypeName, out var inner) && inner.TryGetValue(prototypeID, out var edit))
            {
                yaml.MergeRecursive(edit);
            }
        }

        [DataDefinition]
        public sealed class MergeEdit
        {
            [DataField(required: true)]
            public string Type { get; set; } = default!;

            [DataField(required: true)]
            public List<MappingDataNode> Edits { get; } = default!;
        }
    }
}

using OpenNefia.Content.Portraits;
using OpenNefia.Core.Audio;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Patches;
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
    public sealed class PatchOpTheme : IPatchOperation
    {
        [Dependency] private readonly IResourceManager _resources = default!;

        [DataField(required: true)]
        public string ThemeID { get; } = default!;

        public void Apply(Type prototypeType, string prototypeID, MappingDataNode yaml)
        {
            if (prototypeType == typeof(ChipPrototype)
                || prototypeType == typeof(TilePrototype)
                || prototypeType == typeof(AssetPrototype)
                || prototypeType == typeof(PortraitPrototype))
            {
                if (yaml.TryGet<MappingDataNode>("image", out var imageNode) && imageNode.TryGet<ValueDataNode>("filepath", out var filepathNode))
                {
                    if (filepathNode.Value.StartsWith("/Graphic/Elona/"))
                    {
                        var newPath = filepathNode.Value.Replace("/Graphic/Elona/", $"/Graphic/{ThemeID}/");
                        if (_resources.ContentFileExists(newPath))
                            filepathNode.Value = newPath;
                    }
                }
            }
            else if (prototypeType == typeof(SoundPrototype))
            {
                if (yaml.TryGet<ValueDataNode>("filepath", out var filepathNode))
                {
                    if (filepathNode.Value.StartsWith("/Sound/Elona/"))
                    {
                        var newPath = filepathNode.Value.Replace("/Sound/Elona/", $"/Sound/{ThemeID}/");
                        if (_resources.ContentFileExists(newPath))
                            filepathNode.Value = newPath;
                    }
                }
            }
        }
    }
}

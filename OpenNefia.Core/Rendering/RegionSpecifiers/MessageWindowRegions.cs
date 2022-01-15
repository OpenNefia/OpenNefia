using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class MessageWindowRegions : IRegionSpecifier
    {
        [DataField]
        public Dictionary<string, UIBox2i> Regions { get; } = new();

        public AssetRegions GetRegions(Vector2i size)
        {
            var regions = new AssetRegions();
            foreach(var region in Regions)
            {
                regions[region.Key] = region.Value;
            }
            return regions;
        }
    }
}

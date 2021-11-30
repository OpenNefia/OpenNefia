using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class TopicWindowRegions : IRegionSpecifier
    {
        public AssetRegions GetRegions(Vector2i size)
        {
            var regions = new AssetRegions();
            var (width, height) = size;

            regions["top_mid"] = Box2i.FromDimensions(16, 0, 16, 16);
            regions["bottom_mid"] = Box2i.FromDimensions(16, 32, 16, 16);
            regions["top_mid2"] = Box2i.FromDimensions(16, 0, width % 16, 16);
            regions["bottom_mid2"] = Box2i.FromDimensions(16, 32, width % 16, 16);
            regions["left_mid"] = Box2i.FromDimensions(0, 16, 16, 16);
            regions["right_mid"] = Box2i.FromDimensions(32, 16, 16, 16);
            regions["left_mid2"] = Box2i.FromDimensions(0, 16, 16, height % 16);
            regions["right_mid2"] = Box2i.FromDimensions(32, 16, 16, height % 16);
            regions["top_left"] = Box2i.FromDimensions(0, 0, 16, 16);
            regions["bottom_left"] = Box2i.FromDimensions(0, 32, 16, 16);
            regions["top_right"] = Box2i.FromDimensions(32, 0, 16, 16);
            regions["bottom_right"] = Box2i.FromDimensions(32, 32, 16, 16);

            return regions;
        }
    }
}

using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class CaptionRegions : IRegionSpecifier
    {
        public AssetRegions GetRegions(Vector2i size)
        {
            var regions = new AssetRegions();
            var width = size.X;

            regions["0"] = UIBox2i.FromDimensions(0, 0, 128, 3);
            regions["1"] = UIBox2i.FromDimensions(0, 3, 128, 22);
            regions["2"] = UIBox2i.FromDimensions(0, 0, 128, 2);
            regions["3"] = UIBox2i.FromDimensions(0, 0, width % 128, 3);
            regions["4"] = UIBox2i.FromDimensions(0, 3, width % 128, 22);
            regions["5"] = UIBox2i.FromDimensions(0, 0, width % 128, 2);

            return regions;
        }
    }
}

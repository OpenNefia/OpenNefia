using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class WindowRegions : IRegionSpecifier
    {
        public AssetRegions GetRegions(Vector2i size)
        {
            var regions = new AssetRegions();

            regions["fill"] = Box2i.FromDimensions(24, 24, 228, 144);
            regions["top_left"] = Box2i.FromDimensions(0, 0, 64, 48);
            regions["top_right"] = Box2i.FromDimensions(208, 0, 56, 48);
            regions["bottom_left"] = Box2i.FromDimensions(0, 144, 64, 48);
            regions["bottom_right"] = Box2i.FromDimensions(208, 144, 56, 48);
            for (int i = 0; i < 19; i++)
            {
                regions[$"top_mid_{i}"] = Box2i.FromDimensions(i * 8 + 36, 0, 8, 48);
                regions[$"bottom_mid_{i}"] = Box2i.FromDimensions(i * 8 + 54, 144, 8, 48);
            }

            for (int j = 0; j < 13; j++)
            {
                regions[$"mid_left_{j}"] = Box2i.FromDimensions(0, j * 8 + 48, 64, 8);

                for (int i = 0; i < 19; i++)
                {
                    regions[$"mid_mid_{j}_{i}"] = Box2i.FromDimensions(i * 8 + 64, j * 8 + 48, 8, 8);
                }

                regions[$"mid_right_{j}"] = Box2i.FromDimensions(208, j * 8 + 48, 56, 8);
            }

            return regions;
        }
    }
}

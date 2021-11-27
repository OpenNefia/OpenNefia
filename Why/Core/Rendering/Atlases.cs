using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public static class Atlases
    {
        public static TileAtlas Chip { get; internal set; } = null!;
        public static TileAtlas Tile { get; internal set; } = null!;
    }
}

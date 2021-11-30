using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public interface ITileAtlasManager
    {
        void LoadAtlases();

        public TileAtlas GetAtlas(string name);
    }
}

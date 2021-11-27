using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public interface IAtlasManager
    {
        public TileAtlas GetAtlas(string name);
    }
}

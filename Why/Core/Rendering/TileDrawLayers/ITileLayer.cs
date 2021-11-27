using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public interface ITileLayer : IDrawable
    {
        void OnThemeSwitched();
        void RedrawAll();
        void RedrawDirtyTiles(HashSet<int> dirtyTilesThisTurn);
    }
}

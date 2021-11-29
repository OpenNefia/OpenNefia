using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public abstract class BaseTileLayer : BaseDrawable, ITileLayer
    {
        public abstract void OnThemeSwitched();
        public abstract void RedrawAll();
        public abstract void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn);
    }
}

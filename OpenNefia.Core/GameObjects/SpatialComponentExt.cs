using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    public static class SpatialComponentExt
    {
        public static Vector2 GetScreenPos(this SpatialComponent spatial)
        {
            return GameSession.Coords.TileToScreen(spatial.WorldPosition);
        }
    }
}

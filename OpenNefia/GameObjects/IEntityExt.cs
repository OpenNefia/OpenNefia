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
    public static class IEntityExt
    {
        public static void GetScreenPos(this Entity entity, out Vector2i screenPos)
        {
            GameSession.Coords.TileToScreen(entity.Spatial.WorldPosition, out screenPos);
        }
    }
}

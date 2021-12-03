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
            GameSession.Coords.TileToScreen(entity.Spatial.Pos, out screenPos);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CanSee(this Entity entity, Entity other)
        {
            return entity.HasLos(other.Spatial.Coords);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool HasLos(this Entity entity, MapCoordinates coords)
        {
            return entity.Spatial.Coords.HasLos(coords);
        }
    }
}

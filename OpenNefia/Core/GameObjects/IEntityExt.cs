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
        public static void GetScreenPos(this IEntity entity, out Vector2i screenPos)
        {
            GameSession.Coords.TileToScreen(entity.Pos, out screenPos);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CanSee(this IEntity entity, IEntity other)
        {
            return entity.HasLos(other.Coords);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool CanSee(this IEntity entity, MapCoordinates coords)
        {
            return entity.HasLos(coords);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        public static bool HasLos(this IEntity entity, MapCoordinates coords)
        {
            return entity.Coords.HasLos(coords);
        }
    }
}

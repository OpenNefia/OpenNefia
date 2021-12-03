using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public static class MapCoordinatesExt
    {
        public static IEnumerable<Entity> GetEntities(this MapCoordinates coords)
        {
            if (coords.Map == null)
                return Enumerable.Empty<Entity>();

            return IoCManager.Resolve<IMapManager>().GetEntities(coords);
        }

        private static bool CanSeeThrough(this MapCoordinates coords)
        {
            if (!coords.IsInBounds())
                return false;

            return (coords.Map.TileFlags[coords.X, coords.Y] & TileFlag.IsOpaque) == TileFlag.None;
        }

        private static bool CanPassThrough(this MapCoordinates coords)
        {
            if (!coords.IsInBounds())
                return false;

            return (coords.Map.TileFlags[coords.X, coords.Y] & TileFlag.IsSolid) == TileFlag.None;
        }

        public static bool HasLos(this MapCoordinates from, MapCoordinates to)
        {
            if (!from.IsInBounds() || !to.IsInBounds())
                return false;

            if (from.Map != to.Map)
                return false;

            foreach (var pos in PosHelpers.EnumerateLine(from.Position, to.Position))
            {
                // In Elona, the final tile is visible even if it is solid.
                if (!from.Map.AtPos(pos).CanSeeThrough() && pos != to.Position)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsMemorized(this MapCoordinates coords)
        {
            if (coords.Map == null)
                return false;

            return coords.Map.Tiles[coords.X, coords.Y] == coords.Map.TileMemory[coords.X, coords.Y];
        }

        public static void SetTile(this MapCoordinates coords, PrototypeId<TilePrototype> tileId)
        {
            if (coords.Map == null)
                return;

            coords.Map.SetTile(coords.Position, tileId);
        }

        public static void MemorizeTile(this MapCoordinates coords)
        {
            if (coords.Map == null)
                return;

            coords.Map.MemorizeTile(coords.Position);
        }

        public static bool IsInWindowFov(this MapCoordinates coords)
        {
            if (coords.Map == null || coords.Map != GameSession.ActiveMap)
                return false;

            return coords.Map.IsInWindowFov(coords.Position);
        }

        public static bool CanAccess(this MapCoordinates coords)
        {
            if (coords.Map == null)
                return false;

            return coords.Map.CanAccess(coords.Position);
        }
    }
}

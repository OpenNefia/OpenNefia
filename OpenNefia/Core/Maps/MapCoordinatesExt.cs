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
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public static class MapCoordinatesExt
    {
        public static IEnumerable<IEntity> GetEntities(this MapCoordinates coords)
        {
            return coords.Map?
                .Entities
                .Where(entity => entity.Coords == coords)
                ?? Enumerable.Empty<IEntity>();
        }

        /// <summary>
        /// Gets the primary character on this tile.
        /// 
        /// In Elona, traditionally only one character is allowed on each tile. However, extra features
        /// such as the Riding mechanic or the Tag Teams mechanic added in Elona+ allow multiple characters to
        /// occupy the same tile.
        /// 
        /// This function retrieves the "primary" character used for things like
        /// damage calculation, spell effects, and so on, which should exclude the riding mount, tag team
        /// partner, etc.
        /// 
        /// It's necessary to keep track of the non-primary characters on the same tile because they are 
        /// still affected by things like area of effect magic.
        /// </summary>
        public static IEntity? GetPrimaryChara(this MapCoordinates coords)
        {
            return IoCManager.Resolve<IEntityManager>()
                .EntityQuery<CharaComponent>()
                .Where(chara => chara.Owner.Coords == coords)
                .FirstOrDefault()?.Owner;
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
            if (!from.IsInBounds() || to.IsInBounds())
                return false;

            if (from != to)
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
            if (!coords.IsInBounds())
                return false;

            return coords.Map.Tiles[coords.X, coords.Y] == coords.Map.TileMemory[coords.X, coords.Y];
        }

        public static void SetTile(this MapCoordinates coords, PrototypeId<TilePrototype> tileId)
        {
            if (!coords.IsInBounds())
                return;

            coords.Map.SetTile(coords.Position, tileId);
        }

        public static void MemorizeTile(this MapCoordinates coords)
        {
            if (!coords.IsInBounds())
                return;

            coords.Map.Tiles[coords.X, coords.Y] = coords.Map.TileMemory[coords.X, coords.Y];
        }

        public static bool IsInWindowFov(this MapCoordinates coords)
        {
            if (coords.Map != GameSession.ActiveMap)
                return false;

            if (!coords.IsInBounds())
                return false;

            return coords.Map.IsInWindowFov(coords);
        }
    }
}

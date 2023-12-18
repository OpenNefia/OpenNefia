using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public interface IMap
    {
        MapId Id { get; }
        EntityUid MapEntityUid { get; }
        int Width { get; }
        int Height { get; }
        Vector2i Size { get; }
        UIBox2i Bounds { get; }

        Tile[,] Tiles { get; }
        Tile[,] TileMemory { get; }

        uint LastSightId { get; set; }
        uint[,] InSight { get; }
        MapObjectMemoryStore MapObjectMemory { get; }

        bool NeedsRedraw { get; }
        HashSet<Vector2i> DirtyTilesThisTurn { get; }
        bool RedrawAllThisTurn { get; set; }

        IEnumerable<TileRef> AllTiles { get; }
        IEnumerable<TileRef> AllTileMemory { get; }

        void Clear(PrototypeId<TilePrototype> tile);
        void ClearMemory(PrototypeId<TilePrototype> tile);
        void SetTile(Vector2i pos, PrototypeId<TilePrototype> tile);
        void SetTileMemory(Vector2i pos, PrototypeId<TilePrototype> tile);
        void SetTile(MapCoordinates pos, PrototypeId<TilePrototype> tile);
        void SetTileMemory(MapCoordinates pos, PrototypeId<TilePrototype> tile);

        void RefreshTile(Vector2i pos);
        void RefreshTileEntities(Vector2i pos, IEnumerable<SpatialComponent> entities);

        MapCoordinates AtPos(Vector2i pos);
        MapCoordinates AtPos(int x, int y);
        EntityCoordinates AtPosEntity(Vector2i pos);
        EntityCoordinates AtPosEntity(int x, int y);
        TileRef? GetTile(Vector2i pos);
        TileRef? GetTileMemory(Vector2i pos);
        TileRef? GetTile(MapCoordinates coords);
        TileRef? GetTileMemory(MapCoordinates coords);

        bool IsInWindowFov(Vector2i pos);
        bool IsMemorized(Vector2i pos);
        void MemorizeAllTiles();
        bool IsInBounds(Vector2i position);
        bool IsInBounds(MapCoordinates coords);
        void MemorizeTile(Vector2i position);
        void MemorizeTile(MapCoordinates coords);

        bool CanAccess(Vector2i position);
        bool CanSeeThrough(Vector2i position);
        bool CanAccess(MapCoordinates newPos);
        bool CanSeeThrough(MapCoordinates position);
        bool CanAccess(EntityCoordinates coords, IEntityManager? entityManager = null);
        bool CanSeeThrough(EntityCoordinates coords, IEntityManager? entityManager = null);

        bool HasLineOfSight(Vector2i worldPosition, Vector2i pos);
    }
}

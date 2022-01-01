using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public interface IMap
    {
        EntityUid MapEntityUid { get; }
        MapId Id { get; }
        int Width { get; }
        int Height { get; }
        Vector2i Size { get; }
        UIBox2i Bounds { get; }

        Tile[,] Tiles { get; }
        Tile[,] TileMemory { get; }

        MapObjectMemoryStore MapObjectMemory { get; }
        ShadowMap ShadowMap { get; }
        bool NeedsRedraw { get; }

        IEnumerable<TileRef> AllTiles { get; }
        HashSet<Vector2i> DirtyTilesThisTurn { get; }
        bool RedrawAllThisTurn { get; set; }

        void Clear(PrototypeId<TilePrototype> tile);
        void ClearMemory(PrototypeId<TilePrototype> tile);
        void SetTile(Vector2i pos, PrototypeId<TilePrototype> tile);
        void SetTileMemory(Vector2i pos, PrototypeId<TilePrototype> tile);

        void RefreshTile(Vector2i pos);
        void RefreshTileEntities(Vector2i pos, IEnumerable<Entity> entities);

        MapCoordinates AtPos(Vector2i pos);
        MapCoordinates AtPos(int x, int y);
        EntityCoordinates AtPosEntity(Vector2i pos);
        EntityCoordinates AtPosEntity(int x, int y);
        public TileRef? GetTile(Vector2i pos);
        public TileRef? GetTileMemory(Vector2i pos);
        public TileRef? GetTile(MapCoordinates coords);
        public TileRef? GetTileMemory(MapCoordinates coords);

        bool IsInWindowFov(Vector2i pos);
        bool IsMemorized(Vector2i pos);
        void RefreshVisibility();
        void MemorizeAllTiles();
        bool IsInBounds(Vector2i position);
        bool IsInBounds(MapCoordinates newCoords);
        void MemorizeTile(Vector2i position);

        bool CanAccess(Vector2i position);
        bool CanSeeThrough(Vector2i position);
        bool CanAccess(MapCoordinates newPos);
        bool CanSeeThrough(MapCoordinates position);

        bool HasLineOfSight(Vector2i worldPosition, Vector2i pos);
    }
}
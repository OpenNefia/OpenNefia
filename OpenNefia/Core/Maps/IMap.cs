using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public interface IMap
    {
        MapId Id { get; set; }
        int Width { get; }
        int Height { get; }
        Vector2i Size { get; }

        Tile[,] Tiles { get; }
        Tile[,] TileMemory { get; }
        TileFlag[,] TileFlags { get; }

        MapObjectMemoryStore MapObjectMemory { get; }
        ShadowMap ShadowMap { get; }
        bool NeedsRedraw { get; }

        public IEnumerable<IEntity> Entities { get; }

        IEnumerable<MapCoordinates> AllTiles { get; }
        HashSet<MapCoordinates> DirtyTilesThisTurn { get; }
        bool RedrawAllThisTurn { get; set; }

        void Clear(PrototypeId<TilePrototype> tile);
        void ClearMemory(PrototypeId<TilePrototype> tile);
        void SetTile(Vector2i pos, PrototypeId<TilePrototype> tile);
        void SetTileMemory(Vector2i pos, PrototypeId<TilePrototype> tile);

        void RefreshTile(Vector2i pos);

        MapCoordinates AtPos(Vector2i pos);
        MapCoordinates AtPos(int x, int y);

        bool IsInWindowFov(Vector2i coords);
        void RefreshVisibility();
        void AddEntity(IEntity newEntity);
        void MemorizeAll();
        bool IsInBounds(Vector2i position);
        void MemorizeTile(Vector2i position);
    }
}
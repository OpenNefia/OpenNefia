using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering
{
    public interface ICoords
    {
        public int TileWidth { get; }
        public int TileHeight { get; }
        void GetTiledSize(Vector2i screenSize, out Vector2i tiledSize);
        void TileToScreen(Vector2i tilePos, out Vector2i screenPos);
        void ScreenToTile(Vector2i screenPos, out Vector2i tilePos);
        void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, out Vector2i drawPos);
    }
}
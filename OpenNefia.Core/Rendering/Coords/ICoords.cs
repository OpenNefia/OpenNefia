using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Defines a coordinate projection system for displaying a tilemap on the screen.
    /// </summary>
    /// <remarks>
    /// This is mainly for supporting an isometric mode in the future.
    /// </remarks>
    public interface ICoords
    {
        public Vector2i TileSize { get; }

        void GetTiledSize(Vector2i screenSize, out Vector2i tiledSize);
        void TileToScreen(Vector2i tilePos, out Vector2i screenPos);
        void ScreenToTile(Vector2i screenPos, out Vector2i tilePos);
        void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, out Vector2i drawPos);
    }
}
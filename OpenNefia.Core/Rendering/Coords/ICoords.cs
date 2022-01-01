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

        // TODO: Screen positions need to be float vectors. The reason is high-DPI rendering.

        Vector2i GetTiledSize(Vector2i screenSize);
        Vector2i TileToScreen(Vector2i tilePos);
        Vector2i ScreenToTile(Vector2i screenPos);
        Vector2i BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize);
    }
}
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileRowDrawLayers
{
    /// <summary>
    /// Tile layer that renders inside a single row of tiles.
    /// Because tiles like walls can overlap, this allows for correctly rendering under
    /// occluding tiles.
    /// </summary>
    public interface ITileRowLayer
    {
        void Initialize();
        void OnThemeSwitched();
        void SetMap(IMap map);
        void RedrawAll();
        void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn);
        void Update(float dt);
        void DrawRow(int tileY, int screenX, int screenY);
    }
}

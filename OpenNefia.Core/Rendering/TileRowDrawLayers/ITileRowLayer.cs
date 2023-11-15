using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileRowDrawLayers
{
    public interface ITileRowLayer
    {
        void Initialize();
        void OnThemeSwitched();
        void SetMap(IMap map);
        void RedrawAll();
        void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn);
        void Update(float dt);
        void DrawRow(int y, int screenX, int screenY);
    }
}

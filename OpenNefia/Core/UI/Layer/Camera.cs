using OpenNefia.Core.Map;
using OpenNefia.Core.Object;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public class Camera
    {
        private Map Map;
        private IDrawable Parent;
        private int _ScreenX;
        private int _ScreenY;
        public int ScreenX { get => _ScreenX; }
        public int ScreenY { get => _ScreenY; }

        public Camera(Map map, IDrawable parent)
        {
            Map = map;
            Parent = parent;
            _ScreenX = 0;
            _ScreenY = 0;
        }

        public void CenterOn(int sx, int sy)
        {
            var coords = GraphicsEx.Coords;
            coords.BoundDrawPosition(sx, sy, this.Map.Width, this.Map.Height, this.Parent.Width, this.Parent.Height, out _ScreenX, out _ScreenY);
        }

        public void CenterOn(MapObject obj)
        {
            obj.GetScreenPos(out var sx, out var sy);
            CenterOn(sx, sy);
        }

        public void CenterOn(TilePos pos)
        {
            pos.GetScreenPos(out var sx, out var sy);
            CenterOn(sx, sy);
        }

        public void Pan(int sdx, int sdy)
        {
            this._ScreenX += sdx;
            this._ScreenY += sdy;
        }

        public void TileToVisibleScreen(TilePos pos, out int sx, out int sy)
        {
            pos.GetScreenPos(out sx, out sy);
            sx += ScreenX;
            sy += ScreenY;
        }

        public void VisibleScreenToTile(int sx, int sy, out int tx, out int ty)
        {
            var coords = GraphicsEx.Coords;
            coords.ScreenToTile(sx - this.ScreenX, sy - this.ScreenY, out tx, out ty);
        }
    }
}

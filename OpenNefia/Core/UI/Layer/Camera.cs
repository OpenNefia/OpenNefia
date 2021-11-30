using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public class Camera
    {
        private IMap _map;
        private IDrawable _parent;
        private Vector2i _screenPos;
        public Vector2i ScreenPos { get => _screenPos; set => _screenPos = value; }

        public Camera(IMap map, IDrawable parent)
        {
            _map = map;
            _parent = parent;
        }

        public void CenterOn(Vector2i screenPos)
        {
            GameSession.Coords.BoundDrawPosition(screenPos, _map.Size, this._parent.Size, out _screenPos);
        }

        public void CenterOn(IEntity obj)
        {
            CenterOn(obj.Spatial.Coords);
        }

        public void CenterOn(MapCoordinates coords)
        {
            if (coords.Map != _map)
                return;

            GameSession.Coords.TileToScreen(coords.Position, out var screenPos);
            CenterOn(screenPos);
        }

        public void Pan(Vector2i screenDPos)
        {
            _screenPos += screenDPos;
        }

        public void TileToVisibleScreen(MapCoordinates coords, out Vector2i screenPos)
        {
            GameSession.Coords.TileToScreen(coords.Position, out screenPos);
            _screenPos += screenPos;
        }

        public void VisibleScreenToTile(Vector2i screenPos, out MapCoordinates coords)
        {
            GameSession.Coords.ScreenToTile(screenPos - _screenPos, out var tile);
            coords = new MapCoordinates(_map, tile);
        }
    }
}
